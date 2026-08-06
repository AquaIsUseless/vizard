using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("UI")]
    [SerializeField] private RectTransform mapRect;
    [SerializeField] private RectTransform markerPrefab;

    private readonly Dictionary<int, RectTransform> markers =
        new Dictionary<int, RectTransform>();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        foreach (var pair in markers)
        {
            int spacecraftIndex = pair.Key;
            RectTransform marker = pair.Value;

            //-----------------------------------------
            // Get spacecraft ECI position
            //-----------------------------------------

            double[] pos =
                SpacecraftStateUtilities.GetAbsSpacecraftPositionUnityCS(spacecraftIndex);

            Vector3 eci = new Vector3(
                (float)pos[0],
                (float)pos[1],
                (float)pos[2]);

            //-----------------------------------------
            // Get Earth rotation
            //-----------------------------------------

            Quaternion earthRotation =
                CelestialBodyStateUtilities.GetPlanetRotationUnityCS(0);

            //-----------------------------------------
            // Convert coordinates
            //-----------------------------------------

            Vector3 ecef = ECIToECEF(
                eci,
                earthRotation);

            Vector2 latLon = ECEFToLatLon(ecef);

            marker.anchoredPosition =
                LatLonToMap(latLon.x, latLon.y);
        }
    }

    //----------------------------------------------------
    // Registration
    //----------------------------------------------------

    public void RegisterSpacecraft(int spacecraftIndex)
    {
        if (markers.ContainsKey(spacecraftIndex))
            return;

        RectTransform marker =
            Instantiate(markerPrefab, mapRect);

        marker.name = $"SC{spacecraftIndex}_Marker";

        markers.Add(spacecraftIndex, marker);
    }

    public void RemoveSpacecraft(int spacecraftIndex)
    {
        if (!markers.TryGetValue(spacecraftIndex, out RectTransform marker))
            return;

        Destroy(marker.gameObject);
        markers.Remove(spacecraftIndex);
    }

    //----------------------------------------------------
    // Coordinate conversions
    //----------------------------------------------------

    private Vector3 ECIToECEF(
        Vector3 eci,
        Quaternion earthRotation)
    {
        // If rotation direction is opposite,
        // replace with Quaternion.Inverse(...)
        return earthRotation * eci;
    }

    private Vector2 ECEFToLatLon(Vector3 ecef)
    {
        float r = ecef.magnitude;

        float latitude =
            Mathf.Asin(ecef.y / r) * Mathf.Rad2Deg;

        float longitude =
            Mathf.Atan2(ecef.z, ecef.x) * Mathf.Rad2Deg;

        return new Vector2(latitude, longitude);
    }

    private Vector2 LatLonToMap(
        float latitude,
        float longitude)
    {
        float width = mapRect.rect.width;
        float height = mapRect.rect.height;

        float x =
            (longitude + 180f) / 360f * width;

        float y =
            (90f - latitude) / 180f * height;

        x -= width * 0.5f;
        y -= height * 0.5f;

        return new Vector2(x, y);
    }
}