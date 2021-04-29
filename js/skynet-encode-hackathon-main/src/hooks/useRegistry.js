import { useState } from 'react';
import { SkynetClient, parseSkylink } from 'skynet-js';

const client = new SkynetClient('https://siasky.net');

export const useRegistryWrite = () => {
  const [status, setStatus] = useState('');

  const registryWrite = async (publicKey, privateKey, dataKey, skylink) => {
    try {
      setStatus('setting');

      // generate base32 skylink from base64 skylink
      const rawSkylink = parseSkylink(skylink);

      // Grab the registry entry if it exists and update the revision number
      const { entry } = await client.registry.getEntry(publicKey, dataKey, {
        timeout: 2,
      });
      const revision = entry ? entry.revision + 1n : 0n;

      //build entry
      const updatedEntry = {
        datakey: dataKey,
        data: rawSkylink,
        revision,
      };

      await client.registry.setEntry(privateKey, updatedEntry);

      setStatus('completed');
    } catch (error) {
      setStatus('error');
      console.error(error);
    }
  };

  return [status, registryWrite];
};

export const useRegistryRead = () => {
  const [status, setStatus] = useState('');
  const [registryEntry, setRegistryEntry] = useState({});
  const [registrySignature, setRegistrySignature] = useState('');
  const [encodedSkylink, setEncodedSkylink] = useState('');

  const registryRead = async (publicKey, dataKey) => {
    try {
      setStatus('getting');
      setRegistrySignature({});
      setEncodedSkylink('');

      const { entry, signature } = await client.registry.getEntry(
        publicKey,
        dataKey,
        { timeout: 2 }
      );

      setRegistryEntry(entry);
      //To FIX: not doing a null check -- rather erroring if no reg entry.
      setEncodedSkylink(client.getSkylinkUrl(entry.data));
      setRegistrySignature(signature);

      setStatus('completed');
    } catch (error) {
      setStatus('error');
      console.error(error);
    }
  };

  return [
    registryEntry,
    registrySignature,
    encodedSkylink,
    status,
    registryRead,
  ];
};
