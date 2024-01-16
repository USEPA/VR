using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MX908Attachment : MonoBehaviour {

    [SerializeField] private float height = default;

    private bool    _held;
    private bool    _inTriggerZone;
    private bool    _attached;
    private Vector3 _beltOffset;
    
    private Rigidbody _rigidbody;
    private Transform _transform;
    
    private MX908Controller _parent;
    private Transform       _parentTransform;

    void Awake() {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void InitializeAttachment(MX908Controller parent, Vector3 beltOffset) {
        _parent          = parent;
        _parentTransform = _parent.transform;
        _beltOffset      = beltOffset;
    }
    
    void FixedUpdate() {
        if (_held) return;
        
        Vector3 position;
        
        if (_attached) {
            position = _parentTransform.TransformPoint(_parent.AttachmentOffset);
        } else {
            position = _parent.CameraTransform.TransformPoint(_beltOffset);
            position = new Vector3(position.x, height, position.z);
        }
        
        _rigidbody.MovePosition(position);
        _rigidbody.MoveRotation(_parent.CameraTransform.rotation);
    }

    void OnTriggerEnter(Collider other) {
        if (other.GetComponent<MX908Controller>() != null) {
            _inTriggerZone = true;
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.GetComponent<MX908Controller>() != null) {
            _inTriggerZone = false;
        }
    }

    public void OnSelectEntered(SelectEnterEventArgs args) {
        _held = true;
    }

    public void OnSelectExited(SelectExitEventArgs args) {
        _held = false;

        if (_inTriggerZone) {
            _attached = true;
        }
    }
}
