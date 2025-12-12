using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwapManager : MonoBehaviour
{
    [SerializeField]
    List<string> SceneList = new List<string>();

    public void SwapToTargetScene(int index)
    {
        if (index < 0 || index >= SceneList.Count) return;
        SceneManager.LoadScene(SceneList[index]);
    }
}
