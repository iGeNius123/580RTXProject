using System.Collections.Generic;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour
{
    public ComputeShader RayTracingShader;
    public Texture SkyboxTexture;
    public Light DirectionalLight;
    public Light PointLight;

    [Header("Spheres")]
    public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
    public uint SpheresMax = 2;
    public float SpherePlacementRadius = 100.0f;

    private Camera _camera;
    private float _lastFieldOfView;
    private RenderTexture _target;
    private Material _addMaterial;
    private uint _currentSample = 0;
    private ComputeBuffer _sphereBuffer;
    private List<Transform> _transformsToWatch = new List<Transform>();

    private List<Sphere> spheres  = new List<Sphere>();

    struct Sphere
    {
        public int id;
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
        public List<Photon> photonMap;
    }

    // photon mapping ---------------------------------------------------------
    struct Photon
    {
      float[3] position;
      float[3] direction;
      float[3] energy;
      int sphere_id;
    }

    private Vector3 lightPos = PointLight.transform.position;

    private int numPhotons = 1000; // total number of photons emitted
    private int numBounces = 3; // max number of times each photon bounces
    private float sqRadius = 0.7;
    private float exposure = 50.0;

    private List<Photon> gPhotonMap = new List<Photon>();

    private gIntersect = false; // did photon intersect anything?
    private int gIndex = -1; // id of intersected sphere
    private float gSqDist, gDist = -1.0; // distance from ray to intersection
    private float[3] gPoint = {0.0, 0.0, 0.0}; // point of intersection

    // Function to find sphere intersection
    // ray = location of photon, origin = location of point light
    private void raySphere (Sphere other, float[] ray, float[] origin)
    {
      float[] s = substractXYZ (other.position, origin);
      float radius = other.radius;
      // quadratic function for Distance
      float A = dotProduct (ray, ray);
  		float B = -2.0f * dotProduct (raySphere, ray);
  		float C = dotProduct (raySphere, raySphere) - (radius * radius);
  		float D = B * B - 4 * A * C;

  		if (D > 0.0)
      {
  			float sign = (C < -0.00001) ? 1 : -1;
  			float distance = (-B + sign * Mathf.Sqrt (D)) / (2 * A);
        if (distance < gDist && distance > 0.0)
        {
          gIndex = other.id;
          gDist = distance;
          gIntersect = true;
        }
  		}
    }

    private void rayTrace (float[] ray, float[] origin)
    {
      gIntersect = false;
      gDist = 999999.9;

      foreach (Sphere other in spheres)
      {
        intersectSphere(other, ray, origin);
      }
    }

    private void emitPhotons ()
    {
      Random.seed = 0;
      for (int i = 0; i < nrPhotons ; i++)
      {
        int bounces = 1;
        float[] rgb = { 1.0f, 1.0f, 1.0f };
        float[] ray = normalize3 (randomThree (1.0f));
        float[] prevPoint = lightPos;

        while (prevPoint [1] >= lightPos [1])
        {
          prevPoint = additionXYZ (lightPos, multiplyThree (normalize3 (randomThree (1.0f)), 0.75f));
        }
        // todo: we need to set bounds for the photons; change 1.5 and 1.2
        // this if statement should check if the photon bounces outside the
        // boundary or inside a sphere
        // 1.5 is boundary of cornell box?
        // 1.2 is light source?
        if (abs (prevPoint [0]) > 1.5 || abs (prevPoint [1]) > 1.2f ||
            findSqureDistance (oldPoint, objects.getSphereObject (0), objects.getSphereData (0, 3) * objects.getSphereData (0, 3)))
          bounces = numberOfBounces + 1;

        rayTrace (ray, prevPoint);

        while (gIntersect && bounces <= nrBounces)
        {
          gPoint = additionXYZ (multiplyThree (ray, gDist) , prevPoint);
          rgb = multiplyThree (getColor (rgb, gIndex, 1.0f / Mathf.Sqrt (bounces));
          // todo: i think we need to store which sphere this photon is associated with
          storePhoton (gIndex, gPoint, ray, rgb);
          drawPhoton (ray);
          ray = reflect (ray, prevPoint);
          raytrace (ray, gPoint);
          prevPoint = gPoint;
          bounces++;
        }
      }
    }

    private void storePhoton (int id, float[] position, float[] direction, float[] energy)
    {
      Photon p = new Photon();
      p.position = position;
      p.direction = direction;
      p.energy = energy;
      p.sphere_id = id;
      gPhotonMap.add(p);
    }






    // 3D math functions ------------------------------------------------------
    public float abs (float value)
    {
      return Mathf.Abs (value);
    }

    public float pow (float value, float power)
    {
      return Mathf.Pow (value, power);
    }

    public bool odd (int value)
    {
      return value % 2 != 0;
    }

    public float[] normalize3 (float[] v)
    {
      Vector3 tmp = new Vector3 (v [0], v [1], v [2]).normalized;
      v [0] = tmp.x;
      v [1] = tmp.y;
      v [2] = tmp.z;
      return v;
    }

    public float[] substractXYZ (float[] a, float[] b)
    {
      float[] result = { a [0] - b [0], a [1] - b [1], a [2] - b [2] };
      return result;
    }

    public float[] additionXYZ (float[] a, float[] b)
    {
      float[] result = { a [0] + b [0], a [1] + b [1], a [2] + b [2] };
      return result;
    }

    public float[] multiplyThree (float[] a, float c)
    {
      float[] result = { c * a [0], c * a [1], c * a [2] };
      return result;
    }

    public float dotProduct (float[] a, float[] b)
    {
      return a [0] * b [0] + a [1] * b [1] + a [2] * b [2];
    }

    public float[] randomThree (float s)
    {
      float[] rand = { Random.Range (-s, s), Random.Range (-s, s), Random.Range (-s, s) };
      return rand;
    }

    public bool findSqureDistance (float[] a, float[] b, float sqradius)
    {
      float c = a [0] - b [0];
      float d = c * c;
      if (d > sqradius)
        return false;
      c = a [1] - b [1];
      d += c * c;
      if (d > sqradius)
        return false;
      c = a [2] - b [2];
      d += c * c;
      if (d > sqradius)
        return false;
      variableUtils.setSqDistance (d);
      return true;
    }

    public float lightDiffuse (float[] N, float[] P)
    {
      float[] L = normalize3 (substractXYZ (lightSource, P));
      return dotProduct (N, L);
    }

    public float[] sphereNormal (int idx, float[] P)
    {
      return normalize3 (substractXYZ (P, objects.getSphereObject (idx)));
    }

    public float[] planeNormal (int idx, float[] P, float[] O)
    {
      int axis = (int)objects.getPlaneData (idx, 0);
      float[] N = { 0.0f, 0.0f, 0.0f };
      N [axis] = O [axis] - objects.getPlaneData (idx, 1);
      return normalize3 (N);
    }

    public float[] surfaceNormal (int type, int index, float[] P, float[] Inside)
    {
      if (type == 0) {
        return sphereNormal (index, P);
      } else {
        return planeNormal (index, P, Inside);
      }
    }

    public float lightObject (int type, int idx, float[] P, float lightAmbient)
    {
      float i = lightDiffuse (surfaceNormal (type, idx, P, lightSource), P);
      return  Mathf.Min (1.0f, Mathf.Max (i, lightAmbient));
    }



    private void Awake()
    {
        _camera = GetComponent<Camera>();

        _transformsToWatch.Add(transform);
        _transformsToWatch.Add(Directionalight.transform);

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

    private void SetUpScene()
    {
        // Add a number of random spheres
        for (int i = 0; i < SpheresMax; i++)
        {
            Sphere sphere = new Sphere();

            // Radius and radius
            sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * SpherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);
            sphere.id = i;
            sphere.photonMap = new List<Photon>();

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
    */

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
