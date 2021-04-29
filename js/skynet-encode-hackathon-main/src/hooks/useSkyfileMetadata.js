import { useState } from 'react';
import { SkynetClient } from 'skynet-js';
import { getMetadata } from 'skynet-js/dist/download';

const client = new SkynetClient('https://siasky.net');

const useSkyfileMetadata = () => {
  const [status, setStatus] = useState('');
  const [metadata, setMetadata] = useState({});

  const getMetadata = async (skylink) => {
    try {
      setStatus('downloading');
      setMetadata({});

      const { metadata } = await client.getMetadata(skylink);
      console.log(metadata);

      setMetadata(metadata);
      setStatus('completed');
    } catch (error) {
      setStatus('error');
    }
  };

  return [metadata, status, getMetadata];
};

export default useSkyfileMetadata;
