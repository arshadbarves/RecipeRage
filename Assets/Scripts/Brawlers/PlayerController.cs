using System;
using Brawlers.Abilities;
using Core;
using GameSystem.Input;
using UnityEngine;

namespace Brawlers
{
    public class PlayerController : BaseController
    {
        private static readonly int Speed = Animator.StringToHash("Speed");
        private InputSystem _inputSystem;

        protected override void Awake()
        {
            base.Awake();
            _inputSystem = GameManager.Instance.GetSystem<InputSystem>();

            if (_inputSystem != null)
            {
                _inputSystem.OnPrimaryAttackAction += PrimaryAttack;
                _inputSystem.OnSecondaryAttackAction += SecondaryAttack;
                _inputSystem.OnPrimaryInteractAction += PrimaryInteract;
                _inputSystem.OnSecondaryInteractAction += SecondaryInteract;
            }
            else
            {
                Debug.LogError("InputSystem is not initialized.");
            }
        }

        protected override void Move()
        {
            Vector2 moveInput = GameManager.Instance.GetSystem<InputSystem>().GetMovementInput();
            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
            float moveSpeed = BrawlerData.MoveSpeed * Time.deltaTime;
            CharacterController.Move(moveDirection * moveSpeed);
            Animator.SetFloat(Speed, moveDirection.magnitude);
        }

        protected override void PrimaryAttack()
        {
            throw new NotImplementedException();
        }

        protected override void SecondaryAttack()
        {
            throw new NotImplementedException();
        }

        protected override void PrimaryInteract()
        {
            throw new NotImplementedException();
        }

        protected override void SecondaryInteract()
        {
            throw new NotImplementedException();
        }

        public override Vector3 GetMoveDirection()
        {
            throw new NotImplementedException();
        }

        protected override bool ShouldUseAbility(Ability ability)
        {
            throw new NotImplementedException();
        }
    }
}