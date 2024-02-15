using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TreePooling : MonoBehaviour
{
    public static GameObject[] trees;
    public static int treeIterator = 0;
    public static void CreateTrees(int numTrees, GameObject treeType){
        for (int i = 0; i < numTrees; i++){
            GameObject currentTree = Instantiate(treeType);
            currentTree.SetActive(false);
            trees[i] = currentTree;
        }
        treeIterator = 0;
    }

    public static void ChangeTrees(int numTrees, GameObject treeType){
        for (int i = 0; i < trees.Length; i++){
            trees[i].transform.position = Vector3.zero;
            trees[i].SetActive(false);
        }
        CreateTrees(numTrees, treeType);
    }

    public static GameObject InstantiateTree(){
        treeIterator++;
        return trees[treeIterator];
    }
}
