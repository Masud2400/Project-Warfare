using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;

public class AnimatorExporter
{
    [MenuItem("Tools/Export Animator")]
    static void ExportAnimator()
    {
        Animator animator = Selection.activeGameObject?.GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Select a GameObject with Animator.");
            return;
        }

        AnimatorController controller =
            animator.runtimeAnimatorController as AnimatorController;

        if (controller == null)
        {
            Debug.LogError("Animator Controller not found.");
            return;
        }

        AnimatorData data = new AnimatorData();

        // PARAMETERS
        foreach (var param in controller.parameters)
        {
            data.parameters.Add(new ParameterData
            {
                name = param.name,
                type = param.type.ToString()
            });
        }

        // LAYERS + STATES
        foreach (var layer in controller.layers)
        {
            LayerData layerData = new LayerData();
            layerData.name = layer.name;

            ChildAnimatorState[] states = layer.stateMachine.states;

            foreach (var state in states)
            {
                StateData stateData = new StateData();
                stateData.name = state.state.name;

                foreach (var transition in state.state.transitions)
                {
                    TransitionData transitionData = new TransitionData();

                    if (transition.destinationState != null)
                        transitionData.toState = transition.destinationState.name;

                    transitionData.hasExitTime = transition.hasExitTime;
                    transitionData.duration = transition.duration;

                    foreach (var condition in transition.conditions)
                    {
                        transitionData.conditions.Add(
                            condition.parameter + " " +
                            condition.mode + " " +
                            condition.threshold
                        );
                    }

                    stateData.transitions.Add(transitionData);
                }

                layerData.states.Add(stateData);
            }

            data.layers.Add(layerData);
        }

        string json = JsonUtility.ToJson(data, true);

        string path = Application.dataPath + "/animator_export.json";

        File.WriteAllText(path, json);

        Debug.Log("Animator exported to: " + path);
    }
}

[System.Serializable]
public class AnimatorData
{
    public List<ParameterData> parameters = new();
    public List<LayerData> layers = new();
}

[System.Serializable]
public class ParameterData
{
    public string name;
    public string type;
}

[System.Serializable]
public class LayerData
{
    public string name;
    public List<StateData> states = new();
}

[System.Serializable]
public class StateData
{
    public string name;
    public List<TransitionData> transitions = new();
}

[System.Serializable]
public class TransitionData
{
    public string toState;
    public bool hasExitTime;
    public float duration;
    public List<string> conditions = new();
}
