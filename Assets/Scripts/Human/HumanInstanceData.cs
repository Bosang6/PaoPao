using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "HumanInstanceData", menuName = "PaoPaoOBJ/InstanceData/HumanInstanceData")]
public class HumanInstanceData : PlayerInstanceData
{
    [Header("Input")]
    public InputActionAsset inputActionAsset;
    public string controlScheme;
}
