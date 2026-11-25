using AquaSys.SmoothNormals;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NormalsSmoothingPostProcessor : AssetPostprocessor {
    private const string Tag = "SN";

    private void OnPostprocessModel(GameObject gameObject) {
        if (gameObject.name.Contains(Tag))
            Apply(gameObject);
    }

    public override int GetPostprocessOrder() => 100;


    private void Apply(GameObject gameObject) {
        var meshes = new HashSet<Mesh>();

        const bool includeInactive = true;
        foreach (var meshFilter in gameObject.GetComponentsInChildren<MeshFilter>(includeInactive)) {
            meshes.Add(meshFilter.sharedMesh);
        }

        foreach (var skinnedMeshRenderer in
                 gameObject.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive)) {
            meshes.Add(skinnedMeshRenderer.sharedMesh);
        }

        foreach (var mesh in meshes) {
            mesh.SetUVs(7, AquaSmoothNormals.ComputeSmoothedNormals(mesh));
        }
    }
}
