using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[ExecuteInEditMode]
public class Raytracing_SC_TEST01 : MonoBehaviour
{
    public List<GameObject> Models;
    public ComputeShader RayTracingShader;
    public Texture SkyboxTexture;
    public Light DirectionalLight;

    [Header("Spheres")]
    public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    public uint SpheresMax = 100;
    public float SpherePlacementRadius = 100.0f;

    private Camera _camera;
    private float _lastFieldOfView;
    private RenderTexture _target;
    private Material _addMaterial;
    private uint _currentSample = 0;
    private ComputeBuffer _sphereBuffer;
    private ComputeBuffer _meshObjectBuffer;
    private List<Transform> _transformsToWatch = new List<Transform>();
    private List<Vector4> BoundingSpere;

    struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
    }

    struct MeshObject
    {
        public Vector3 v1;
    }

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        _transformsToWatch.Add(transform);
        _transformsToWatch.Add(DirectionalLight.transform);
    }

    private void OnEnable()
    {
        _currentSample = 0;
        SetUpScene();
    }

    private void OnDisable()
    {
        if (_sphereBuffer != null)
            _sphereBuffer.Release();
    }

    private void Update()
    {
        if (_camera.fieldOfView != _lastFieldOfView)
        {
            _currentSample = 0;
            _lastFieldOfView = _camera.fieldOfView;
        }

        foreach (Transform t in _transformsToWatch)
        {
            if (t.hasChanged)
            {
                _currentSample = 0;
                t.hasChanged = false;
            }
        }
    }

    private void SetupMeshObjects()
    {
        BoundingSpere = new List<Vector4>();

        foreach (GameObject model in Models)
        {
            Mesh mesh = model.GetComponent<MeshFilter>().sharedMesh;
            float maxX = -Mathf.Infinity, maxY = -Mathf.Infinity, maxZ = -Mathf.Infinity, minX = Mathf.Infinity, minY = Mathf.Infinity, minZ = Mathf.Infinity;

            foreach (var vert in mesh.vertices)
            {
                if (vert.x > maxX) maxX = vert.x;
                if (vert.y > maxY) maxY = vert.y;
                if (vert.z > maxZ) maxZ = vert.z;
                if (vert.x < minX) minX = vert.x;
                if (vert.y < minY) minY = vert.y;
                if (vert.z < minZ) minZ = vert.z;
            }

            float x = maxX - minX;
            float y = maxY - minY;
            float z = maxZ - minZ;

            Vector3 origin = new Vector3(0.5f * (maxX + minX), 0.5f * (maxY + minY), 0.5f * (maxZ + minZ));

            float r = x > y ? x * 0.5f : y * 0.5f;
            r = r > z ? r : z * 0.5f;

            foreach (var vert in mesh.vertices)
            {
                if (Vector3.Distance(vert, origin) > r)
                    r = Vector3.Distance(vert, origin);
            }
            BoundingSpere.Add(new Vector4(origin.x, origin.y, origin.z, r));
        }

        // Assign to compute buffer
        if (_meshObjectBuffer != null)
            _meshObjectBuffer.Release();
        if (spheres.Count > 0)
        {
            _meshObjectBuffer = new ComputeBuffer(spheres.Count, 40);
            _meshObjectBuffer.SetData(spheres);
        }
    }

    private void SetupGeneratedSpheres()
    {
        List<Sphere> spheres = new List<Sphere>();

        // Add a number of random spheres
        for (int i = 0; i < SpheresMax; i++)
        {
            Sphere sphere = new Sphere();

            // Radius and radius
            sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * SpherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);

            // Reject spheres that are intersecting others
            foreach (Sphere other in spheres)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                    goto SkipSphere;
            }

            // Albedo and specular color
            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.5f;
            sphere.albedo = metal ? Vector4.zero : new Vector4(color.r, color.g, color.b);
            sphere.specular = metal ? new Vector4(color.r, color.g, color.b) : new Vector4(0.04f, 0.04f, 0.04f);

            // Add the sphere to the list
            spheres.Add(sphere);

        SkipSphere:
            continue;
        }

        // Assign to compute buffer
        if (_sphereBuffer != null)
            _sphereBuffer.Release();
        if (spheres.Count > 0)
        {
            _sphereBuffer = new ComputeBuffer(spheres.Count, 40);
            _sphereBuffer.SetData(spheres);
        }
    }

    private void SetUpScene()
    {
        SetupGeneratedSpheres();
        SetupMeshObjects();
    }

    private void SetShaderParameters()
    {
        RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
        RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
        RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
        RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));

        Vector3 l = DirectionalLight.transform.forward;
        RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

        if (_sphereBuffer != null)
            RayTracingShader.SetBuffer(0, "_Spheres", _sphereBuffer);

        if (_meshObjectBuffer != null)
            RayTracingShader.SetBuffer(0, "_MeshObjectBuffer", _meshObjectBuffer); 
    }

    private void InitRenderTexture()
    {
        if (_target == null || _target.width != Screen.width || _target.height != Screen.height)
        {
            // Release render texture if we already have one
            if (_target != null)
                _target.Release();

            // Get a render target for Ray Tracing
            _target = new RenderTexture(Screen.width, Screen.height, 0,
                RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            _target.enableRandomWrite = true;
            _target.Create();

            // Reset sampling
            _currentSample = 0;
        }
    }

    private void Render(RenderTexture destination)
    {
        // Make sure we have a current render target
        InitRenderTexture();

        // Set the target and dispatch the compute shader
        RayTracingShader.SetTexture(0, "Result", _target);
        int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);
        RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

        // Blit the result texture to the screen
        if (_addMaterial == null)
            _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        _addMaterial.SetFloat("_Sample", _currentSample);
        Graphics.Blit(_target, destination, _addMaterial);
        _currentSample++;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        SetShaderParameters();
        Render(destination);
    }
}
