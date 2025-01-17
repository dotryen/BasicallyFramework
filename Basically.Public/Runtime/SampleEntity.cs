﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Basically.Entities;
using Basically.Utility;

// There are functions left out as they are typically overriden to achieve something specific
public class SampleEntity : PredictableEntity {
    #region Client Callbacks
    #if BASICALLY_CLIENT

    protected internal override void OnClientStart() {

    }

    protected internal override void OnClientTick() {
        
    }

    protected internal override void OnClientUpdate() {
        
    }

    protected internal override void OnClientLateUpdate() {
        
    }

    #endif
    #endregion

    #region Server Callbacks
    #if BASICALLY_SERVER

    protected internal override void OnServerStart() {
        
    }

    protected internal override void OnServerTick() {
        
    }

    #endif
    #endregion

    #region Entity Callbacks

    protected internal override void Predict() {
        
    }

    protected internal override void InterpFunc(EntityState from, EntityState to, float interpAmount) {
        base.InterpFunc(from, to, interpAmount);
    }

    #endregion

    #region Serializing/Deseriazing

    protected internal override void Serialize(ref IParameters parameters) {

    }

    protected internal override void Deserialize(IParameters parameters) {

    }

    #endregion
}
