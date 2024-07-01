using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Thirdweb;
using System.Threading.Tasks;
using Thirdweb.Examples;

public class NftManager : MonoBehaviour
{
    [SerializeField] private ThirdwebManager _thirdwebManager;
    private ThirdwebSDK sdk;
    private NFT nft;
    private Contract contract;
    public Prefab_NFT prefab_NFT;
    //public TMP_Text confirm;

    // Start is called before the first frame update
    private async void Start()
    {
        sdk = _thirdwebManager.SDK;
        contract = sdk.GetContract("0xFb1Eb0e44ae5298BE4e23C1ab7C807d6158B934C");
        //GetNFTMedia();
        Debug.Log("Started");
        //await(CheckBalance());
        Debug.Log("Started");
    }

    public async Task<string> CheckBalance()
    {
        contract = sdk.GetContract("0xFb1Eb0e44ae5298BE4e23C1ab7C807d6158B934C");
        string balance = await contract.Read<string>("balanceof", "0x39691E1C32Ed33EBd775f51AA2568f1eD7eBCe7E", 0);
        print (balance);
        return balance;
    }
    
    public async void GetNFTMedia()
    {
        await (CheckBalance());
        nft = await contract.ERC1155.Get("0");
        Prefab_NFT prefabNftScript = prefab_NFT.GetComponent<Prefab_NFT>();
        prefabNftScript.LoadNFT(nft);
        //confirm.text = nft.metadata.name;
        Debug.Log(nft.metadata.name);
    }
}
