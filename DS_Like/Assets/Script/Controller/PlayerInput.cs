using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerInput : MonoBehaviour
{
    protected static PlayerInput m_Instance;
    public static PlayerInput Instance
    {
        get { return m_Instance; }
    }

    [HideInInspector]
    public bool PlayerControllerInputBlocked;

    private CinemachineBrain m_CamBrain;

    private Vector2 m_Movement;
    private Vector2 m_Camera;
    private bool m_Sprint;
    private bool m_Jump;
    private bool m_Attack;
    private bool m_Roll;
    private bool m_Pause;
    private bool m_ExternalInputBlocked;

    public Vector2 MoveInput
    {
        get
        {
            if (PlayerControllerInputBlocked || m_ExternalInputBlocked)
            {
                return Vector2.zero;
            }
            return m_Movement;
        }
    }

    public Vector2 CameraInput
    {
        get
        {
            if (PlayerControllerInputBlocked || m_ExternalInputBlocked)
            {
                return Vector2.zero;
            }
            return m_Camera;
        }
    }

    public bool SprintInput { get => m_Sprint && !PlayerControllerInputBlocked && !m_ExternalInputBlocked; }
    public bool JumpInput { get => m_Jump && !PlayerControllerInputBlocked && !m_ExternalInputBlocked; }
    public bool RollInput { get => m_Roll && !PlayerControllerInputBlocked && !m_ExternalInputBlocked; }
    public bool AttackInput { get => m_Attack && !PlayerControllerInputBlocked && !m_ExternalInputBlocked; }
    public bool PauseInput { get => m_Pause; }

    private void Awake() 
    {
        if (m_Instance == null)
        {
            m_Instance = this;
        }
        else if (m_Instance != this)
        {
            Destroy(this);
            throw new UnityException("There cannot be more than on PlayerInput script in the scene");
        }
    }

    private void Start() 
    {
        Camera cam = Camera.main;
        m_CamBrain = cam.GetComponent<CinemachineBrain>();
    }

    private void Update() 
    {
        m_Movement.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        m_Camera.Set(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        m_Sprint = Input.GetButton("Sprint");
        m_Jump = Input.GetButtonDown("Jump");
        m_Roll = Input.GetButtonDown("Roll");
        m_Attack = Input.GetButtonDown("Fire1");

        m_Pause = Input.GetButtonDown("Pause");
    }

    public bool HaveControl()
    {
        return !m_ExternalInputBlocked;
    }

    public void ReleaseControl()
    {
        m_CamBrain.enabled = false;
        m_ExternalInputBlocked = true;
    }

    public void GainControl()
    {
        m_CamBrain.enabled = true;
        m_ExternalInputBlocked = false;
    }
}