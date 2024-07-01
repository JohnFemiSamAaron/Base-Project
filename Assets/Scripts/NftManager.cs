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

    private String address;
    //public TMP_Text confirm;

    // Start is called before the first frame update
    private async void Start()
    {
        sdk = _thirdwebManager.SDK;
        contract = sdk.GetContract("0xFb1Eb0e44ae5298BE4e23C1ab7C807d6158B934C");
        address = await _thirdwebManager.SDK.Wallet.GetAddress();
        //GetNFTMedia();
        Debug.Log("Started");
        //await(CheckBalance());
        Debug.Log("Started");
    }

    public async Task<string> CheckBalance()
    {
        contract = sdk.GetContract("0xDE3727A531423ccF41cb5c1007172384a6736a74");
        string balance = await contract.Read<string>("balanceof", "0xDE3727A531423ccF41cb5c1007172384a6736a74", 0);
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
    
    public async void claimNFT()
    {
        contract = sdk.GetContract("0xDE3727A531423ccF41cb5c1007172384a6736a74");
        await contract.ERC1155.ClaimTo(address, "0", 1);
        
        //Give Ability to then highlight like the rest
    }
}
