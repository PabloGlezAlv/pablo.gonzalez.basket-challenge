using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

[CustomEditor(typeof(CameraMenuController))]
public class CameraMenuControllerEditor : Editor
{
    private ReorderableList cameraSetupsList;
    private SerializedProperty moveSpeed;
    private SerializedProperty moveCurve;
    private SerializedProperty playerAnimatorController;
    private SerializedProperty gameLocation;

    private void OnEnable()
    {
        SerializedProperty cameraSetups = serializedObject.FindProperty("cameraSetups");
        moveSpeed = serializedObject.FindProperty("moveSpeed");
        moveCurve = serializedObject.FindProperty("moveCurve");
        playerAnimatorController = serializedObject.FindProperty("playerAnimatorController");
        gameLocation = serializedObject.FindProperty("gameLocation");

        cameraSetupsList = new ReorderableList(serializedObject, cameraSetups, true, true, true, true);

        cameraSetupsList.drawHeaderCallback = (Rect rect) => {
            EditorGUI.LabelField(rect, "Camera Setups");
        };

        cameraSetupsList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) => {
            SerializedProperty element = cameraSetupsList.serializedProperty.GetArrayElementAtIndex(index);

            rect.y += 2;
            float lineHeight = EditorGUIUtility.singleLineHeight;

            Rect stateRect = new Rect(rect.x, rect.y, rect.width, lineHeight);
            EditorGUI.PropertyField(stateRect, element.FindPropertyRelative("state"), new GUIContent("State"));

            Rect positionRect = new Rect(rect.x, rect.y + lineHeight + 2, rect.width, lineHeight);
            EditorGUI.PropertyField(positionRect, element.FindPropertyRelative("position"), new GUIContent("Position"));

            Rect uiPanelRect = new Rect(rect.x, rect.y + (lineHeight + 2) * 2, rect.width, lineHeight);
            EditorGUI.PropertyField(uiPanelRect, element.FindPropertyRelative("uiPanel"), new GUIContent("UI Panel"));
        };

        cameraSetupsList.elementHeight = EditorGUIUtility.singleLineHeight * 3 + 6;

        cameraSetupsList.onAddCallback = (ReorderableList list) => {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;

            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("state").enumValueIndex = 0;
            element.FindPropertyRelative("position").objectReferenceValue = null;
            element.FindPropertyRelative("uiPanel").objectReferenceValue = null;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Camera Controller Settings", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        cameraSetupsList.DoLayoutList();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Movement Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(moveSpeed);
        EditorGUILayout.PropertyField(moveCurve);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Character Components", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(playerAnimatorController);
        EditorGUILayout.PropertyField(gameLocation);
        EditorGUILayout.Space();

        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Runtime Controls", EditorStyles.boldLabel);

            CameraMenuController controller = (CameraMenuController)target;

            EditorGUILayout.LabelField($"Current State: {controller.GetCurrentState()}");
            EditorGUILayout.LabelField($"Is Moving: {controller.IsMoving()}");

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Move to Menu"))
            {
                controller.MoveToMenu();
            }
            if (GUILayout.Button("Move to Gameplay"))
            {
                controller.MoveToGameplay();
            }
            if (GUILayout.Button("Move to Reward"))
            {
                controller.MoveToReward();
            }
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }
}