using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    protected static PlayerInput m_Instance;
    public static PlayerInput Instance
    {
        get { return m_Instance; }
    }

    [HideInInspector]
    public bool PlayerControllerInputBlocked;

    protected Vector2 m_Movement;
    protected Vector2 m_Camera;
    protected bool m_Jump;
    protected bool m_Attack;
    protected bool m_Pause;
    protected bool m_ExternalInputBlocked;

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

    public bool JumpInput
    {
        get
        {
            return m_Jump && !PlayerControllerInputBlocked && !m_ExternalInputBlocked;
        }
    }

    public bool AttackInput
    {
        get
        {
            return m_Attack && !PlayerControllerInputBlocked && !m_ExternalInputBlocked;
        }
    }

    public bool PauseInput
    {
        get { return m_Pause; }
    }

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
    }

    private void Update() 
    {
        m_Movement.Set(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        m_Camera.Set(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        m_Jump = Input.GetButtonDown("Jump");
        m_Attack = Input.GetButtonDown("Fire1");

        m_Pause = Input.GetButtonDown("Pause");
    }

    public bool HaveControl()
    {
        return !m_ExternalInputBlocked;
    }

    public void ReleaseControl()
    {
        m_ExternalInputBlocked = true;
    }

    public void GainControl()
    {
        m_ExternalInputBlocked = false;
    }
}