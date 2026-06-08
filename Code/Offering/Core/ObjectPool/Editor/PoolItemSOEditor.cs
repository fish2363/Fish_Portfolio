using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

    [CustomEditor(typeof(PoolItemSO))]
    public class PoolItemSOEditor : UnityEditor.Editor
    {
        [SerializeField] private VisualTreeAsset visualTreeAsset = default;
        
        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = new VisualElement();
            visualTreeAsset.CloneTree(root);
            
            TextField nameField = root.Q<TextField>("PoolNameField");
            nameField.RegisterValueChangedCallback(HandleAssetNameChange);

            return root;
        }

        private void HandleAssetNameChange(ChangeEvent<string> evt)
        {
            if (string.IsNullOrEmpty(evt.newValue))
            {
                EditorUtility.DisplayDialog("Error", "Name cannot be empty", "OK");
                (evt.target as TextField).SetValueWithoutNotify(evt.previousValue);
                return;
            }

            //현재 SO의 경로를 알아낸다.
            string assetPath = AssetDatabase.GetAssetPath(target);
            string newName = $"{evt.newValue}"; //신규 파일이름

            string message = AssetDatabase.RenameAsset(assetPath, newName); //새로운 이름으로 다시 저장
            if (string.IsNullOrEmpty(message)) //메시지가 없다는건 성공적으로 교체되었다는 뜻
            {
                target.name = newName;
            }
            else
            {
                (evt.target as TextField).SetValueWithoutNotify(evt.previousValue);
                EditorUtility.DisplayDialog("Error", message, "OK");
            }
        }
    }
