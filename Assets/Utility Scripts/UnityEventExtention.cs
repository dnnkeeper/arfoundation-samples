using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


[System.Serializable]
public class UnityEventVector3 : UnityEvent<Vector3>
{

}

[System.Serializable]
public class UnityEventGameObject : UnityEvent<GameObject>
{

}

[System.Serializable]
public class UnityEventPointerEventData : UnityEvent<PointerEventData>
{

}


[System.Serializable]
public class UnityEventInt : UnityEvent<int>
{

}

[System.Serializable]
public class UnityEventFloat : UnityEvent<float>
{

}

[System.Serializable]
public class UnityEventString : UnityEvent<string>
{

}


[System.Serializable]
public class UnityPositionRotationEvent : UnityEvent<Vector3, Quaternion>
{

}

