using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Zeftarim.ThirdPerson.EditorTools
{
    public class ModelSwapperWindow : EditorWindow
    {
        [MenuItem("Tools/Model Swapper Tool")]
        public static void ShowWindow()
        {
            GetWindow<ModelSwapperWindow>("Model Swapper");
        }

        private Animator oldModelAnimator;
        private Animator newModelAnimator;

        private Transform bodyAimTransform;
        private Transform aimTargetTransform;

        private MultiAimConstraint headAim;
        private MultiAimConstraint neckAim;
        private MultiAimConstraint spineUpperAim;
        private MultiAimConstraint spineLowerAim;

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("1. Models", EditorStyles.boldLabel);
            oldModelAnimator = (Animator)EditorGUILayout.ObjectField("Old Model (Current)", oldModelAnimator, typeof(Animator), true);
            newModelAnimator = (Animator)EditorGUILayout.ObjectField("New Model (FBX)", newModelAnimator, typeof(Animator), true);

            GUILayout.Space(10);
            GUILayout.Label("2. Rigging Objects (Must be inside Old Model)", EditorStyles.boldLabel);
            bodyAimTransform = (Transform)EditorGUILayout.ObjectField("BodyAim Object", bodyAimTransform, typeof(Transform), true);
            aimTargetTransform = (Transform)EditorGUILayout.ObjectField("AIM Object (Target)", aimTargetTransform, typeof(Transform), true);

            GUILayout.Space(10);
            GUILayout.Label("3. Constraints to Rebind", EditorStyles.boldLabel);
            headAim = (MultiAimConstraint)EditorGUILayout.ObjectField("Head Aim", headAim, typeof(MultiAimConstraint), true);
            neckAim = (MultiAimConstraint)EditorGUILayout.ObjectField("Neck Aim", neckAim, typeof(MultiAimConstraint), true);
            spineUpperAim = (MultiAimConstraint)EditorGUILayout.ObjectField("Spine Upper Aim", spineUpperAim, typeof(MultiAimConstraint), true);
            spineLowerAim = (MultiAimConstraint)EditorGUILayout.ObjectField("Spine Lower Aim", spineLowerAim, typeof(MultiAimConstraint), true);

            GUILayout.Space(20);

            GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
            if (GUILayout.Button("Migrate Rigging to New Model", GUILayout.Height(40)))
            {
                ExecuteSwap();
            }
            GUI.backgroundColor = Color.white;
        }

        private void ExecuteSwap()
        {
            if (oldModelAnimator == null || newModelAnimator == null || bodyAimTransform == null || aimTargetTransform == null)
            {
                EditorUtility.DisplayDialog("Error", "Missing references to assign.", "OK");
                return;
            }

            Undo.SetTransformParent(bodyAimTransform, newModelAnimator.transform, "Move BodyAim");
            Undo.SetTransformParent(aimTargetTransform, newModelAnimator.transform, "Move AIM Target");

            RigBuilder newRigBuilder = newModelAnimator.GetComponent<RigBuilder>();
            if (newRigBuilder == null)
            {
                newRigBuilder = Undo.AddComponent<RigBuilder>(newModelAnimator.gameObject);
            }

            newRigBuilder.layers.Clear();
            Rig rigComponent = bodyAimTransform.GetComponent<Rig>();
            if (rigComponent != null)
            {
                newRigBuilder.layers.Add(new RigLayer(rigComponent));
            }

            Transform headBone = newModelAnimator.GetBoneTransform(HumanBodyBones.Head);
            Transform neckBone = newModelAnimator.GetBoneTransform(HumanBodyBones.Neck);
            Transform chestBone = newModelAnimator.GetBoneTransform(HumanBodyBones.Chest);
            Transform spineBone = newModelAnimator.GetBoneTransform(HumanBodyBones.Spine);

            RebindConstraint(headAim, headBone);
            RebindConstraint(neckAim, neckBone);
            RebindConstraint(spineUpperAim, chestBone);
            RebindConstraint(spineLowerAim, spineBone);

            newRigBuilder.Build();

            RigBuilder oldRigBuilder = oldModelAnimator.GetComponent<RigBuilder>();
            if (oldRigBuilder != null)
            {
                Undo.DestroyObjectImmediate(oldRigBuilder);
            }

            EditorUtility.DisplayDialog("Success", "Rigging successfully transferred and rebound to the new model.\n\nYou can now delete the old model from the scene.", "OK");
        }

        private void RebindConstraint(MultiAimConstraint constraint, Transform newBone)
        {
            if (constraint != null && newBone != null)
            {
                Undo.RecordObject(constraint, "Rebind Constraint");
                var data = constraint.data;
                data.constrainedObject = newBone;
                constraint.data = data;
            }
        }
    }
}