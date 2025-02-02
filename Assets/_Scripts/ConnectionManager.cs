using FishNet.Managing;
using UnityEngine;
using TMPro;
using Edgegap.Editor.Api.Models.Results;
using com.cyborgAssets.inspectorButtonPro;
using FishNet.Transporting.Bayou;
using Edgegap.Editor.Api.Models.Requests;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Events;
using System;
using Newtonsoft.Json;

public class ConnectionManager : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;
    [SerializeField] Bayou bayou;
    [SerializeField] TMP_Text statusText;

    [ProButton]
    void Start()
    {
        StartCoroutine(DeployAndConnect());
    }

    IEnumerator DeployAndConnect()
    {
        // get list of deployments
        yield return new WaitForSeconds(2);
        if (statusText != null) statusText.text = "loading";
        ListAllDeploymentsResult listAllDeploymentsResult = new();
        yield return EdgegapApiGet<ListAllDeploymentsResult>("/v1/deployments", (d) => listAllDeploymentsResult = d);

        string fqdn = "";
        ushort port = 0;

        // check if there is a deployment available
        if (listAllDeploymentsResult.data.Count > 0)
        {
            // connect to existing deployment
            if (statusText != null) statusText.text = "Connecting";

            // get deployment info
            GetDeploymentStatusResult getDeploymentStatusResult = new();
            yield return EdgegapApiGet<GetDeploymentStatusResult>($"/v1/status/{listAllDeploymentsResult.data[0].request_id}", d => getDeploymentStatusResult = d);
            fqdn = getDeploymentStatusResult.Fqdn;
            port = (ushort)getDeploymentStatusResult.PortsDict["Game Port"].External;
        }
        else
        {
            // get my public ip
            if (statusText != null) statusText.text = "Deploying";

            GetYourPublicIpResult ipResult = new();
            yield return EdgegapApiGet<GetYourPublicIpResult>("/v1/ip", d =>
            {
                Debug.Log($"(d) public ip: {d.PublicIp}");
                ipResult = d;
            });


            // create new deployment
            CreateDeploymentRequest createDeploymentRequest = new("FishNetTest", "0.0.2", ipResult.PublicIp);

            CreateDeploymentResult createDeploymentResult = new();
            yield return EdgegapApiPost<CreateDeploymentRequest, CreateDeploymentResult>("/v1/deploy", createDeploymentRequest, d => createDeploymentResult = d);

            if (statusText != null) statusText.text = "Connecting";
            GetDeploymentStatusResult getDeploymentStatusResult = new();
            yield return EdgegapApiGet<GetDeploymentStatusResult>($"/v1/status/{createDeploymentResult.RequestId}", d => getDeploymentStatusResult = d);
            fqdn = getDeploymentStatusResult.Fqdn;
            port = (ushort)getDeploymentStatusResult.PortsDict["Game Port"].External;
        }

        if (Application.isPlaying)
        {
            bayou.SetClientAddress(fqdn);
            bayou.SetPort((ushort)port);
            if (networkManager != null)
                networkManager.ClientManager.StartConnection();
            if (statusText != null) statusText.text = "Connected!";

            yield return new WaitForSeconds(1);
        }
        if (statusText != null) statusText.text = "";
    }

    IEnumerator EdgegapApiGet<K>(string endpoint, UnityAction<K> responseData = null)
    {
        // send request
        UnityWebRequest www = UnityWebRequest.Get($"https://api.edgegap.com{endpoint}");
        www.SetRequestHeader("authorization", "token 4e7f0c54-c4e4-4692-8f19-a1441220cfb4");
        yield return www.SendWebRequest();
        while (!www.isDone) yield return new WaitForEndOfFrame();
        if (www.result != UnityWebRequest.Result.Success)
            throw new System.Exception($"Error for EdgegapApiGet (\"{endpoint}\"): {www.error}");

        // get data
        if (responseData != null)
        {
            string json = RemoveEmptyArrays(www.downloadHandler.text);
            K data;
            try
            {
                data = JsonConvert.DeserializeObject<K>(json);
            }
            catch (Exception e)
            {
                data = JsonUtility.FromJson<K>(json);
            }
            responseData?.Invoke(data);
        }
    }

    IEnumerator EdgegapApiPost<T, K>(string endpoint, T requestData, UnityAction<K> responseData = null)
    {
        // send request
        string requestString;
        try
        {
            requestString = JsonConvert.SerializeObject(requestData);
        }
        catch (Exception e)
        {
            requestString = JsonUtility.ToJson(requestData);
        }
        Debug.Log(requestString);
        UnityWebRequest www = UnityWebRequest.Post($"https://api.edgegap.com{endpoint}", requestString, "application/json");
        www.SetRequestHeader("authorization", "token 4e7f0c54-c4e4-4692-8f19-a1441220cfb4");
        yield return www.SendWebRequest();
        while (!www.isDone) yield return new WaitForEndOfFrame();
        if (www.result != UnityWebRequest.Result.Success)
            throw new System.Exception($"Error for EdgegapApiPost (\"{endpoint}\"): {www.error}");

        // get data
        if (responseData != null)
        {
            string json = RemoveEmptyArrays(www.downloadHandler.text);
            K data;
            try
            {
                data = JsonConvert.DeserializeObject<K>(json);
            }
            catch (Exception e)
            {
                data = JsonUtility.FromJson<K>(json);
            }
            responseData?.Invoke(data);
        }
    }

    public static string RemoveEmptyArrays(string json)
    {
        // remove empty arrays
        var emptyArray = "[]";
        var emptyObject = "[]";
        int numLoops = 1000;
        while ((json.Contains(emptyArray) || json.Contains(emptyObject)) && numLoops > 0)
        {
            numLoops--;
            // get start of index
            int index = json.IndexOf(emptyArray);

            // find second " to remove the name and previous comma (if it exists)
            int quotationMarkNumber = 0;
            int startIndex = index;
            int endIndex = index + 1;
            var jsonChar = json.ToCharArray();
            while (quotationMarkNumber < 2 && startIndex > 0)
            {
                startIndex--;
                if (jsonChar[startIndex] == '"')
                    quotationMarkNumber++;
            }

            // get any escape characters
            if (jsonChar[startIndex - 1] == '\\')
                startIndex--;

            // get commas
            if (endIndex + 1 < jsonChar.Length && jsonChar[endIndex + 1] == ',')
                endIndex++;
            else if (startIndex - 1 > 0 && jsonChar[startIndex - 1] == ',')
                startIndex--;
            else if (startIndex - 2 > 0 && jsonChar[startIndex - 2] == ',')
                startIndex -= 2;

            // get spaces
            if (startIndex - 1 > 0 && jsonChar[startIndex - 1] == ' ')
                startIndex--;
            else if (endIndex + 1 > 0 && jsonChar[endIndex + 1] == ' ')
                endIndex++;

            Debug.Log($"Removing: \"{json.Substring(startIndex, endIndex - startIndex + 1)}\", final values at ('{jsonChar[startIndex]}', '{jsonChar[endIndex]}')");
            json = json.Remove(startIndex, endIndex - startIndex + 1);
        }

        return json;
    }
}
