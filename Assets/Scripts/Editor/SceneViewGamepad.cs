using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[InitializeOnLoad]
public static class SceneViewGamepad
{
    static float moveSpeed       = 10f;
    static float lookSpeed       = 120f;
    static float boostMultiplier = 3f;

    static SceneViewGamepad()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        var gamepad = Gamepad.current;
        if (gamepad == null) {
            return;
        }
		
        var sceneView = SceneView.lastActiveSceneView;
        if (sceneView == null) {
            return;
        }

        InputSystem.Update();

        Vector2 leftStick  = gamepad.leftStick.ReadValue();
        Vector2 rightStick = gamepad.rightStick.ReadValue();
        float leftTrigger  = gamepad.leftTrigger.ReadValue();
        float rightTrigger = gamepad.rightTrigger.ReadValue();
        float vertical     = rightTrigger - leftTrigger;

        if (Mathf.Abs(leftStick.x) < 0.1f)   leftStick.x = 0f;
        if (Mathf.Abs(leftStick.y) < 0.1f)   leftStick.y = 0f;
        if (Mathf.Abs(rightStick.x) < 0.15f) rightStick.x = 0f;
        if (Mathf.Abs(rightStick.y) < 0.15f) rightStick.y = 0f;
        if (Mathf.Abs(vertical) < 0.1f)      vertical = 0f;
        if (leftStick == Vector2.zero && rightStick == Vector2.zero && vertical == 0f) {
            return;
        }

        float dt    = 0.016f;
        bool boost  = gamepad.leftShoulder.isPressed || gamepad.rightShoulder.isPressed;
        float speed = moveSpeed * (boost ? boostMultiplier : 1f);

        var pivot    = sceneView.pivot;
        var rotation = sceneView.rotation;

        Vector3 forward = rotation * Vector3.forward;
        Vector3 right   = rotation * Vector3.right;

        pivot += right * (leftStick.x * speed * dt);
        pivot += forward * (leftStick.y * speed * dt);
        pivot += Vector3.up * (vertical * speed * dt);

        float yaw   = rightStick.x * lookSpeed * dt;
        float pitch = -rightStick.y * lookSpeed * dt;

        rotation = Quaternion.Euler(0, yaw, 0) * rotation;
        rotation = rotation * Quaternion.Euler(pitch, 0, 0);

        sceneView.pivot    = pivot;
        sceneView.rotation = rotation;
        
		sceneView.Repaint();
    }
}
