using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public sealed class AIManager
{
    #region Variables
    private static AIManager _aiManager;

    public static AIManager Instance
    {
        get
        {
            if (_aiManager == null)
                _aiManager = new AIManager();
            return _aiManager;
        }
    }

    private Transform _playerTransform;

    public Transform PlayerTransform
    {
        get
        {
            if (_playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    _playerTransform = player.transform;
            }
            return _playerTransform;
                
        }
    }
    #endregion
}
