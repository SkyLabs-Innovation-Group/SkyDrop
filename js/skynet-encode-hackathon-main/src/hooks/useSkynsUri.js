import { parseSkylink, SkynetClient } from 'skynet-js';
import { convertSkylinkToBase32 } from 'skynet-js/dist/utils';
import { useState } from 'react';

const client = new SkynetClient();

const useSkynsUri = () => {
  const [skynsUri, setSkynsUri] = useState('');

  const parseURI = (url) => {
    if (!url) return;

    // Split the url into the query keypairs
    let keypairs = url.split('?')[1].split('&');
    let pk = keypairs[0];
    let dk = keypairs[1];
    return `skyns://${pk.split('=')[1]}/${dk.split('=')[1]}`;
  };

  const skynsUriConverter = (publicKey, dataKey) => {
    const entryUrl = client.registry.getEntryUrl(publicKey, dataKey);
    setSkynsUri(parseURI(entryUrl));
  };

  return [skynsUri, skynsUriConverter];
};

export default useSkynsUri;
