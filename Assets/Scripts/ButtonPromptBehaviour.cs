using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPromptBehaviour : MonoBehaviour
{
    [SerializeField] Canvas ButtonPromptToActivate;
    [Tooltip("Time until the prompt disappears in seconds")]
    [SerializeField] private float _timeToDeactivate;
    private float _timer = 0;
    private bool _isActivated = false;
    private bool _done = false;


    void Update()
    {
        if(_isActivated)
        {
            if(_timer >= _timeToDeactivate)
            {
                _done = true;
                _isActivated = false;
                ButtonPromptToActivate.enabled = false;
            }
            _timer += Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && !_isActivated && !_done)
        {
            _isActivated = true;
            ButtonPromptToActivate.enabled = true;
        }
    }
}
