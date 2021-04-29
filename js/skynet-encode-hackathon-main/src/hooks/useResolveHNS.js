import { useState } from "react";
import { SkynetClient } from "skynet-js";
import { makeUrl, addUrlQuery, defaultOptions } from "skynet-js/dist/utils";
import axios from "axios";

//adjusted SDK code follows:

const defaultGetEntryOptions = {
  ...defaultOptions("/skynet/registry"),
  timeout: 5
};

class ModifiedClient extends SkynetClient {
  getEntryUrl({ publickey, datakey }, customOptions = {}) {
    const opts = {
      ...defaultGetEntryOptions,
      ...this.customOptions,
      ...customOptions
    };

    const timeout = opts.timeout;

    const query = {
      publickey,
      datakey,
      timeout
    };

    let url = makeUrl(this.portalUrl, opts.endpointPath);
    url = addUrlQuery(url, query);

    return url;
  }

  async resolveRegistry(registryUrl) {
    // code ported from lua on webportal

    // -- since /skynet/registry endpoint response is a json, we need to decode it before we access it
    // local registry_json = json.decode(registry_res.body)
    // -- response will contain a hex encoded skylink, we need to decode it
    // local data = (registry_json.data:gsub('..', function (cc)
    //   return string.char(tonumber(cc, 16))
    // end))

    //get resistry entry
    const resp = await axios.get(registryUrl);

    //hex string to buffer data
    const output = Buffer.from(resp.data.data, "hex");

    return { skylink: output.toString("utf8") };

    //
  }
}

const useResolveHNS = (portals) => {
  const [status, setStatus] = useState("");
  const [registryEntry, setRegistryEntry] = useState("");
  const [resolvedHNSURL, setResolvedHNSURL] = useState("");

  const client = new ModifiedClient("https://siasky.net");

  const resolveHNS = async (hnsName) => {
    try {
      setResolvedHNSURL("");
      setRegistryEntry("");

      if (!hnsName) {
        setStatus("empty");
        return;
      }

      setStatus("checking");
      const txtRecord = await client.resolveHns(hnsName);

      if ("registry" in txtRecord) {
        console.log("found registry entry");

        // show registry object
        // console.log(txtRecord["registry"]);

        // use custom getEntryUrl that doesn't hash datakey
        const entryUrl = await client.getEntryUrl(txtRecord["registry"]);
        setRegistryEntry(entryUrl);
        // console.log(entryUrl);

        // use custom resolveRegistry that looks up registry entry and makes skylink
        const skylink = await client.resolveRegistry(entryUrl);
        // console.log(skylink);

        const url = client.getSkylinkUrl(skylink.skylink);
        // console.log(url);

        setResolvedHNSURL(url);
      } else if ("skylink" in txtRecord) {
        console.log("found skylink entry");

        // simply convert skylink to URL and set
        const url = client.getSkylinkUrl(txtRecord.skylink);
        setResolvedHNSURL(url);
      }
      setStatus("completed");
    } catch (error) {
      console.log(error);
      setStatus("error");
    }
  };

  return [resolvedHNSURL, registryEntry, status, resolveHNS];
};

export default useResolveHNS;
