using KitchenClash.Infrastructure.Input;
using Unity.Netcode;
using UnityEngine;
using KitchenClash.Domain;


namespace KitchenClash.Infrastructure.Network
{
    public partial class PlayerController
    {
        #region Input Setup

        private void SetupInput()
        {
            _inputProvider = InputProviderFactory.CreateForPlatform();
            if (_inputProvider == null)
            {
                GameLogger.LogError("Input provider not found");
                return;
            }

            _inputProvider.OnMovementInput += HandleMoveInput;
            _inputProvider.OnInteractionInput += HandleInteractInput;
            _inputProvider.OnSpecialAbilityInput += HandleAbilityInput;
        }

        private void HandleMoveInput(Vector2 input)
        {
            _inputHandler?.SetRawInput(input);
        }

        private void HandleInteractInput()
        {
            if (!IsLocalPlayer)
            {
                return;
            }

            bool interacted = _interactionController?.TryInteract(_stateController, this) ?? false;
            if (interacted)
            {
                InteractServerRpc();
            }
        }

        private void HandleAbilityInput()
        {
            if (!IsLocalPlayer)
            {
                return;
            }

            bool used = _interactionController?.TryUseAbility(PrimaryAbility, _stateController) ?? false;
            if (used)
            {
                UseAbilityServerRpc();
            }
        }

        #endregion

        #region Movement Processing

        private void ProcessMovement()
        {
            if (_inputHandler == null || _movementController == null || _stateController == null)
            {
                return;
            }

            Vector2 input = _inputEnabled ? _inputHandler.GetSmoothedInput() : Vector2.zero;
            _movementController.ApplyMovement(input, _stateController.CurrentState, Time.fixedDeltaTime);
        }

        private void ProcessMovementWithPrediction()
        {
            if (_inputHandler == null || _movementController == null || _stateController == null || _networkController == null)
            {
                return;
            }

            Vector2 input = _inputEnabled ? _inputHandler.GetSmoothedInput() : Vector2.zero;

            PlayerInputData inputData = _networkController.CreateInputData(input);
            _movementController.ApplyMovement(input, _stateController.CurrentState, Time.fixedDeltaTime);

            PlayerStateData stateData = _networkController.CreateStateData(transform, _rigidbody, inputData.SequenceNumber);
            _networkController.StoreHistory(inputData, stateData);

            SendInputToServerRpc(inputData);
        }

        #endregion

        #region Network RPCs

        [ServerRpc]
        private void SendInputToServerRpc(PlayerInputData input)
        {
            if (_movementController == null || _stateController == null || _networkController == null)
            {
                return;
            }

            _movementController.ApplyMovement(input.Movement, _stateController.CurrentState, Time.fixedDeltaTime);
            PlayerStateData authState = _networkController.CreateStateData(transform, _rigidbody, input.SequenceNumber);
            ReconcileStateClientRpc(authState);
        }

        [ClientRpc]
        private void ReconcileStateClientRpc(PlayerStateData serverState)
        {
            if (IsServer || _networkController == null || _movementController == null || _stateController == null)
            {
                return;
            }

            _networkController.ReconcileState(
                serverState,
                transform,
                _rigidbody,
                (input) => _movementController.ApplyMovement(input.Movement, _stateController.CurrentState, Time.fixedDeltaTime)
            );
        }

        [ServerRpc]
        private void InteractServerRpc() => InteractClientRpc();

        [ClientRpc]
        private void InteractClientRpc()
        {
            if (IsLocalPlayer)
            {
                return;
            }
        }

        [ServerRpc]
        private void UseAbilityServerRpc() => UseAbilityClientRpc();

        [ClientRpc]
        private void UseAbilityClientRpc()
        {
            if (IsLocalPlayer)
            {
                return;
            }
        }

        #endregion

    }
}
