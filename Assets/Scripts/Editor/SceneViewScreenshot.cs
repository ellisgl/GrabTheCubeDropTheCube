using System.IO;
using UnityEditor;
using UnityEngine;

public static class SceneViewScreenshot
{
    [MenuItem("Tools/Save Scene View Screenshot")]
    static void Save()
    {
        SceneView view = SceneView.lastActiveSceneView;
        if (view == null)
        {
            Debug.LogWarning("No Scene view is open.");
            return;
        }

        const int width = 1920;
        const int height = 1080;

        Camera camera = view.camera;
        RenderTexture target = new RenderTexture(width, height, 24);
        RenderTexture previousTarget = camera.targetTexture;
        RenderTexture previousActive = RenderTexture.active;

        camera.targetTexture = target;
        camera.Render();

        RenderTexture.active = target;
        Texture2D image = new Texture2D(width, height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        image.Apply();

        camera.targetTexture = previousTarget;
        RenderTexture.active = previousActive;

        // Saved next to the Assets folder, so Unity doesn't import it
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots",
            "SceneView-" + System.DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".png");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, image.EncodeToPNG());

        Object.DestroyImmediate(image);
        Object.DestroyImmediate(target);
        Debug.Log("Saved " + path);
    }
}
