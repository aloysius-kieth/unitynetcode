using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour {

    [SerializeField] private Transform spawnedObjectPrefab;

    private Transform spawnedTransform;

    private NetworkVariable<CustomData> randomNumber = new NetworkVariable<CustomData>(new CustomData {
        _int = 0,
        _bool = false,
    }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //private NetworkVariable<string> _string = new NetworkVariable<string>("temp", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct CustomData : INetworkSerializable {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        randomNumber.OnValueChanged += (CustomData prev, CustomData next) => {
            Debug.Log(OwnerClientId + "; randomNumber: " + next._int + "; randomBool: " + next._bool + "; string" + next.message);
        };
        //_string.OnValueChanged += (string prev, string next) => {
        //    Debug.Log(OwnerClientId + "; _string:" + next);
        //};
    }


    private void Update() {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.T)) {
            spawnedTransform = Instantiate(spawnedObjectPrefab);
            spawnedTransform.GetComponent<NetworkObject>().Spawn(true);
            //TestClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } } });
            //randomNumber.Value = new CustomData {
            //    _int = Random.Range(0, 100),
            //    _bool = false,
            //    message = "all your base belongs to us!"
            //};
        }

        if (Input.GetKeyDown(KeyCode.Y)) {
            spawnedTransform.GetComponent<NetworkObject>().Despawn(true);
        }

        //if (Input.GetKeyDown(KeyCode.M)) {
        //    _string.Value = "test" + Random.Range(0, 100).ToString();
        //}

        Vector3 moveDir = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) moveDir.z += 1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z -= 1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x -= 1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x += 1f;

        float moveSpeed = 8f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }

    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams) {
        Debug.Log("TestServerRpc: " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams) {
        Debug.Log("Test Client Rpc");
    }
}
