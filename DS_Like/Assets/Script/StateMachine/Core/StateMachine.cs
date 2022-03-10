using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TNT.StateMachine
{
    public class StateMachine
    {
        private class StateInfo
        {
            /// <summary>
            /// If true, it will wait one frame before entering the update state.
            /// Otherwise, then OnEnter and OnUpdate will be called on the same frame
            /// </summary>
            public bool m_DelayOneFrameUpdate;

            public System.Action m_OnEnter;
            public System.Action m_OnUpdate;
            public System.Action m_OnFixedUpdate;
            public System.Action m_OnExit;

            public void OnEnter(System.Action i_OnEnter)
            {
                m_OnEnter = i_OnEnter;
            }
            public void OnUpdate(System.Action i_OnUpdate, bool i_DelayOneFrameUpdate = false)
            {
                m_DelayOneFrameUpdate = i_DelayOneFrameUpdate;
                m_OnUpdate = i_OnUpdate;
            }
            public void OnFixedUpdate(System.Action i_OnFixedUpdate)
            {
                m_OnFixedUpdate = i_OnFixedUpdate;
            }
            public void OnExit(System.Action i_OnExit)
            {
                m_OnExit = i_OnExit;
            }
        }

        private int m_PreviousState = -1;
        private int m_CurrentState = 0;
        private int m_NextState = -1;

#if UNITY_EDITOR
        private bool m_IsInited;
#endif
        private bool m_CanUpdate;
        private bool m_CanFixedUpdate;
        private bool m_DelayUpdate;

        private Dictionary<int, StateInfo> m_StateInfos = new Dictionary<int, StateInfo>();

        public int PreviousState
        {
            get { return m_PreviousState; }
        }

        public int CurrentState
        {
            get { return m_CurrentState; }
        }
        
        public int NextState
        {
            get { return m_NextState; }
        }

        public void AddState(int i_State)
        {
            if (!m_StateInfos.ContainsKey(i_State))
            {
                m_StateInfos.Add(i_State, new StateInfo());
            }
            else
            {
                Debug.LogError("Cannot AddState since it already exists!");
            }
        }

        public void AddMultipleState(int[] i_States)
        {
            for (int i = 0; i < i_States.Length; i++)
            {
                if (!m_StateInfos.ContainsKey(i_States[i]))
                {
                    m_StateInfos.Add(i_States[i], new StateInfo());
                }
                else
                {
                    Debug.LogError("Cannot AddState: " + i_States[i] + " ,since it already exists!");
                }
            }
        }

        public void OnEnter(int i_State, System.Action i_OnEnter)
        {
            if (m_StateInfos.ContainsKey(i_State))
            {
                m_StateInfos[i_State].OnEnter(i_OnEnter);
            }
            else
            {
                Debug.LogError("Cannot SetOnEnterAction to State " + i_State.ToString() + " since the state doesn't exist! Use 'AddState'");
            }
        }

        public void OnUpdate(int i_State, System.Action i_OnUpdate, bool i_DelayOneFrameUpdate = false)
        {
            if (m_StateInfos.ContainsKey(i_State))
            {
                m_StateInfos[i_State].OnUpdate(i_OnUpdate, i_DelayOneFrameUpdate);
            }
            else
            {
                Debug.LogError("Cannot SetOnUpdateAction to State " + i_State.ToString() + " since the state doesn't exist! Use 'AddState'");
            }
        }

        public void OnFixedUpdate(int i_State, System.Action i_OnFixedUpdate)
        {
            if (m_StateInfos.ContainsKey(i_State))
            {
                m_StateInfos[i_State].OnFixedUpdate(i_OnFixedUpdate);
            }
            else
            {
                Debug.LogError("Cannot SetOnFixedUpdateAction to State " + i_State.ToString() + " since the state doesn't exist! Use 'AddState'");
            }
        }

        public void OnExit(int i_State, System.Action i_OnExit)
        {
            if (m_StateInfos.ContainsKey(i_State))
            {
                m_StateInfos[i_State].OnExit(i_OnExit);
            }
            else
            {
                Debug.LogError("Cannot SetOnExitAction to State " + i_State.ToString() + " since the state doesn't exist! Use 'AddState'");
            }
        }

        public void Init(int i_InitialState)
        {
            if (m_StateInfos.ContainsKey(i_InitialState))
            {
#if UNITY_EDITOR
                m_IsInited = true;
#endif
                SetupState(i_InitialState);
            }
            else
            {
                Debug.LogError("Cannot Init to State " + i_InitialState.ToString() + " since the state doesn't exist! Use 'AddState'");
            }
        }

        public void ChangeState(int i_NextState)
        {
            if (m_CurrentState == i_NextState)
            {
                Debug.LogWarning("Cannot Change State to " + i_NextState.ToString() + " since it's already in this state");
                return;
            }

            m_NextState = i_NextState;
            m_CanUpdate = false;
            m_CanFixedUpdate = false;
            if (m_StateInfos.ContainsKey(m_CurrentState))
            {
                if (m_StateInfos[m_CurrentState].m_OnExit != null)
                {
                    m_StateInfos[m_CurrentState].m_OnExit();
                }
            }

            SetupState(i_NextState);
        }

        private void SetupState(int i_State)
        {
            m_PreviousState = m_CurrentState;
            m_CurrentState = i_State;
            m_NextState = -1;

            if (m_StateInfos.ContainsKey(m_CurrentState))
            {
                if (m_StateInfos[m_CurrentState].m_OnEnter != null)
                {
                    //Debug.Log("Enter State = " + m_CurrentState.ToString());
                    m_StateInfos[m_CurrentState].m_OnEnter();
                }
                if (m_StateInfos[m_CurrentState].m_OnUpdate != null)
                {
                    m_DelayUpdate = m_StateInfos[m_CurrentState].m_DelayOneFrameUpdate;
                    m_CanUpdate = true;
                }
                if (m_StateInfos[m_CurrentState].m_OnFixedUpdate != null)
                {
                    m_CanFixedUpdate = true;
                }
            }
        }

        /// <summary>
        /// Must be called in the Update method in the component using it
        /// </summary>
        public void UpdateSM()
        {
            if (m_CanUpdate)
            {
#if UNITY_EDITOR
                if (!m_IsInited)
                {
                    Debug.LogWarning("State Machine isn't inited. UpdateSM will nto be called. You must call the Init() method after initialising the State Machine!");
                }
#endif
                //Debug.Log("Update State = " + m_CurrentState.ToString());
                if(m_DelayUpdate)
                {
                    m_DelayUpdate = false;
                    return;
                }
                m_StateInfos[m_CurrentState].m_OnUpdate();
            }
        }

        public void FixedUpdateSM()
        {
            if (m_CanFixedUpdate)
            {
#if UNITY_EDITOR
                if (!m_IsInited)
                {
                    Debug.LogWarning("State Machine isn't inited. FixedUpdateSM will nto be called. You must call the Init() method after initialising the State Machine!");
                }
#endif
                //Debug.Log("Update State = " + m_CurrentState.ToString());
                m_StateInfos[m_CurrentState].m_OnFixedUpdate();
            }
        }
    }
}