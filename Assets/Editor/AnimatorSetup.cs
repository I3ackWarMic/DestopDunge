#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

// Editor utility to create simple animation clips and populate the base AnimatorController
// Creates clips: Idle, Transition, Attack and sets them as states in AC_DefaultCharacter.controller
public static class AnimatorSetup
{
    [MenuItem("DestopDunge/Animator/Generate Sample Clips & Controller")]
    public static void Generate()
    {
        string animatorPath = "Assets/Animator/AC_DefaultCharacter.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorPath);
        if (controller == null)
        {
            Debug.LogError("Base AnimatorController not found at " + animatorPath + " — run Setup Project first.");
            return;
        }

        // create clips folder
        string clipsFolder = "Assets/Animator/Clips";
        if (!AssetDatabase.IsValidFolder(clipsFolder)) AssetDatabase.CreateFolder("Assets/Animator", "Clips");

        // Idle clip: slight bobbing via localPosition.y
        string idlePath = clipsFolder + "/idle.anim";
        AnimationClip idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(idlePath);
        if (idleClip == null)
        {
            idleClip = new AnimationClip();
            var curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0.05f);
            idleClip.SetCurve("", typeof(Transform), "localPosition.y", curve);
            idleClip.frameRate = 30;
            AssetDatabase.CreateAsset(idleClip, idlePath);
            Debug.Log("Created idle clip: " + idlePath);
        }

        // Transition clip: quick scale up
        string transPath = clipsFolder + "/transition.anim";
        AnimationClip transClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(transPath);
        if (transClip == null)
        {
            transClip = new AnimationClip();
            var curveX = AnimationCurve.EaseInOut(0f, 1f, 0.3f, 1.2f);
            var curveY = AnimationCurve.EaseInOut(0f, 1f, 0.3f, 1.2f);
            transClip.SetCurve("", typeof(Transform), "localScale.x", curveX);
            transClip.SetCurve("", typeof(Transform), "localScale.y", curveY);
            transClip.frameRate = 30;
            AssetDatabase.CreateAsset(transClip, transPath);
            Debug.Log("Created transition clip: " + transPath);
        }

        // Attack clip: short forward motion
        string attackPath = clipsFolder + "/attack.anim";
        AnimationClip attackClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(attackPath);
        if (attackClip == null)
        {
            attackClip = new AnimationClip();
            var curve = AnimationCurve.EaseInOut(0f, 0f, 0.2f, 0.5f);
            attackClip.SetCurve("", typeof(Transform), "localPosition.x", curve);
            attackClip.frameRate = 30;
            AssetDatabase.CreateAsset(attackClip, attackPath);
            Debug.Log("Created attack clip: " + attackPath);
        }

        // Ensure states exist in controller
        var root = controller.layers[0].stateMachine;
        void AddStateIfMissing(string name, AnimationClip clip)
        {
            var state = root.states;
            foreach (var s in state)
            {
                if (s.state.name == name) return;
            }
            var newState = root.AddState(name);
            newState.motion = clip;
            Debug.Log("Added state '" + name + "' to controller");
        }

        AddStateIfMissing("Idle", idleClip);
        AddStateIfMissing("Transition", transClip);
        AddStateIfMissing("CombatIdle", idleClip);
        AddStateIfMissing("Attack", attackClip);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Animator sample clips and states created / added to " + animatorPath);
        EditorUtility.DisplayDialog("Animator Setup", "Generated sample clips and added states to AC_DefaultCharacter.controller", "OK");
    }
}
#endif
