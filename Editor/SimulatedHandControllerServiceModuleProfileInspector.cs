﻿// Copyright (c) Reality Collective. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using RealityCollective.Editor.Extensions;
using RealityToolkit.DeviceSimulation.InputService.HandTracking;
using RealityToolkit.Input.Hands.Poses;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RealityToolkit.DeviceSimulation.Editor
{
    [CustomEditor(typeof(SimulatedHandControllerServiceModuleProfile))]
    public class SimulatedHandControllerServiceModuleProfileInspector : SimulatedControllerServiceModuleProfileInspector
    {
        private static readonly GUIContent SimulatedHandSettingsFoldoutHeader = new GUIContent("Simulated Hand Tracking Settings");
        private static readonly GUIContent handPoseAnimationSpeedLabel = new GUIContent("Hand Pose Animation Speed");

        private SerializedProperty gripThreshold;
        private SerializedProperty handPhysicsEnabled;
        private SerializedProperty useTriggers;
        private SerializedProperty boundsMode;
        private SerializedProperty trackedPoses;
        private SerializedProperty handPoseAnimationSpeed;

        private bool showSimulatedHandTrackingSettings = true;
        private ReorderableList poseProfilesList;
        private int currentlySelectedPoseElement;

        protected override void OnEnable()
        {
            base.OnEnable();

            gripThreshold = serializedObject.FindProperty(nameof(gripThreshold));
            handPhysicsEnabled = serializedObject.FindProperty(nameof(handPhysicsEnabled));
            useTriggers = serializedObject.FindProperty(nameof(useTriggers));
            boundsMode = serializedObject.FindProperty(nameof(boundsMode));

            trackedPoses = serializedObject.FindProperty(nameof(trackedPoses));

            poseProfilesList = new ReorderableList(serializedObject, trackedPoses, true, false, true, true)
            {
                elementHeight = EditorGUIUtility.singleLineHeight * 1.5f
            };
            poseProfilesList.drawHeaderCallback += PoseProfilesList_DrawHeaderCallback;
            poseProfilesList.drawElementCallback += PoseProfilesList_DrawConfigurationOptionElement;
            poseProfilesList.onAddCallback += PoseProfilesList_OnConfigurationOptionAdded;
            poseProfilesList.onRemoveCallback += PoseProfilesList_OnConfigurationOptionRemoved;

            handPoseAnimationSpeed = serializedObject.FindProperty(nameof(handPoseAnimationSpeed));
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EditorGUILayout.Space();

            showSimulatedHandTrackingSettings = EditorGUILayoutExtensions.FoldoutWithBoldLabel(showSimulatedHandTrackingSettings, SimulatedHandSettingsFoldoutHeader);

            if (showSimulatedHandTrackingSettings)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.LabelField("General Hand Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(gripThreshold);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Hand Physics Settings", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(handPhysicsEnabled);
                EditorGUILayout.PropertyField(useTriggers);
                EditorGUILayout.PropertyField(boundsMode);
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel++;
                poseProfilesList.DoLayoutList();
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("Simulated Poses", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                handPoseAnimationSpeed.floatValue = EditorGUILayout.Slider(handPoseAnimationSpeedLabel, handPoseAnimationSpeed.floatValue, 1, 10);
                EditorGUILayout.Space();
                EditorGUI.indentLevel--;

                EditorGUI.indentLevel--;

                serializedObject.ApplyModifiedProperties();
            }
        }

        private void PoseProfilesList_DrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, "Tracked Hand Poses");
        }

        private void PoseProfilesList_DrawConfigurationOptionElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (isFocused)
            {
                currentlySelectedPoseElement = index;
            }

            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += 3;
            var poseDataProperty = trackedPoses.GetArrayElementAtIndex(index);
            var selectedPoseData = EditorGUI.ObjectField(rect, poseDataProperty.objectReferenceValue, typeof(HandControllerPoseProfile), false) as HandControllerPoseProfile;

            if (selectedPoseData != null)
            {
                selectedPoseData.ParentProfile = ThisProfile;
            }

            poseDataProperty.objectReferenceValue = selectedPoseData;
        }

        private void PoseProfilesList_OnConfigurationOptionAdded(ReorderableList list)
        {
            trackedPoses.arraySize += 1;
            var index = trackedPoses.arraySize - 1;

            var mappingProfileProperty = trackedPoses.GetArrayElementAtIndex(index);
            mappingProfileProperty.objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
        }

        private void PoseProfilesList_OnConfigurationOptionRemoved(ReorderableList list)
        {
            if (currentlySelectedPoseElement >= 0)
            {
                trackedPoses.DeleteArrayElementAtIndex(currentlySelectedPoseElement);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}