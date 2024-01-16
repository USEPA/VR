using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MX908Controller : MonoBehaviour {
    [SerializeField] private MX908Attachment[] attachments      = default;
    [SerializeField] private Vector3[]         beltOffsets      = default;
    [SerializeField] private Vector3           attachmentOffset = default;
    [SerializeField] private Vector3           cameraOffset     = default;
    [SerializeField] private float             height           = default;
    
    private Rigidbody _rigidbody;
    private Transform _transform;
    private Transform _cameraTransform;
    
    void Awake() {
        _transform = transform;
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    void Start() {
        _cameraTransform = Camera.main.transform;
        
        for (var i = 0; i < attachments.Length; i++) {
            attachments[i].InitializeAttachment(this, beltOffsets[i]);
        }
    }

    void FixedUpdate() {
        var position = _cameraTransform.TransformPoint(cameraOffset);
        _rigidbody.MovePosition(new Vector3(position.x, height, position.z));
        _rigidbody.MoveRotation(_cameraTransform.rotation);
    }

    public Vector3   AttachmentOffset => attachmentOffset;
    public Transform CameraTransform  => _cameraTransform;
}
