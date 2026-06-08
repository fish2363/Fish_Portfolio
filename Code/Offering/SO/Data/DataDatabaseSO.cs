//using System.Collections.Generic;
//using UnityEngine;

//[CreateAssetMenu(menuName = "Game/Data/Data Database", fileName = "DataDatabase")]
//public class DataDatabaseSO : ScriptableObject
//{
//    [Tooltip("IData를 구현한 ScriptableObject들을 여기에 넣어주세요.")]
//    public List<ScriptableObject> entries = new();

//    public IEnumerable<IData> EnumerateData()
//    {
//        foreach (var so in entries)
//        {
//            if (so is IData data)
//                yield return data;
//        }
//    }
//}