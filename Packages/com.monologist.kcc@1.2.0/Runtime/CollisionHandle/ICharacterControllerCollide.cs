using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monologist.KCC
{
    public interface ICharacterControllerCollide
    {
        public void OnCharacterControllerCollide(CharacterCollision collision);
    }
}

