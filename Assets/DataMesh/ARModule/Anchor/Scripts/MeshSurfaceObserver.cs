// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;

namespace DataMesh.AR.Anchor
{
    public class MeshSurfaceObserver : SpatialMappingSource
    {
        private GameObject objects;

        [Tooltip("If greater than or equal to zero, surface objects will claim to be updated at this period. This is useful when working with libraries that respond to updates (such as the SpatialUnderstanding library). If negative, surfaces will not claim to be updated.")]
        public float SimulatedUpdatePeriodInSeconds = -1;

        // Use this for initialization.
        private void Start()
        {
        }

        public void SetMeshes(List<Mesh> meshes)
        {
            Cleanup();

            for (int i = 0;i < meshes.Count; i ++)
            {
                Mesh mesh = meshes[i];
                SurfaceObject surface = CreateSurfaceObject(mesh, "storedmesh-" + SurfaceObjects.Count, transform, i);
                AddSurfaceObject(surface);

                GameObject obj = surface.Object;
                Renderer renderer = obj.GetComponent<MeshRenderer>();

                if (SpatialMappingManager.Instance.DrawVisualMeshes == false)
                {
                    renderer.enabled = false;
                }

                if (SpatialMappingManager.Instance.CastShadows == false)
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }

                MeshCollider collider = obj.GetComponent<MeshCollider>();
                collider.sharedMesh = null;
                collider.sharedMesh = obj.GetComponent<MeshFilter>().mesh;
            }

            if (GetMeshFilters().Count > 0)
            {
                SpatialMappingManager.Instance.SetSpatialMappingSource(this);
            }
        }

        private float lastUpdateUnscaledTimeInSeconds = 0;

        private void Update()
        {
            if (SimulatedUpdatePeriodInSeconds >= 0)
            {
                if ((Time.unscaledTime - lastUpdateUnscaledTimeInSeconds) >= SimulatedUpdatePeriodInSeconds)
                {
                    for (int iSurface = 0; iSurface < SurfaceObjects.Count; iSurface++)
                    {
                        UpdateOrAddSurfaceObject(SurfaceObjects[iSurface]);
                    }

                    lastUpdateUnscaledTimeInSeconds = Time.unscaledTime;
                }
            }
        }
    }
}