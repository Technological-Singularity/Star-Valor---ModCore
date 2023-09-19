using System;
using System.Reflection;
using UnityEngine;
using Charon.StarValor.ModCore;

namespace Charon.StarValor.Excursion {
    [Serializable]
    public class UncontrolledShipCharacter : AIDummyCharacter {
        protected override Type AIControlType => typeof(UncontrollShipControl);
        public UncontrolledShipCharacter(SpaceShipData shipData) : base(shipData) { }
    }
}
