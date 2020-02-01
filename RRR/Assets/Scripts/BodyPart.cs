﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BodyPart
{
    public enum BodyPartType
    {
        HORN_1,
        HORN_2,
        BODY_1,
        BODY_2,
        TAIL_1,
        TAIL_2
    }

    private float _health;

    public float Health
    {
        get { return _health; }
    }

    [SerializeField]
    private BodyPartType _type;
    public BodyPartType Type => _type;

    public BodyPart(BodyPartType type)
    {
        _type = type;
        _health = Config.initialBodyPartHealth;
    }
}