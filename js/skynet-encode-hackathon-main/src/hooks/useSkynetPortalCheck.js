import { useState } from 'react';
import { SkynetClient } from 'skynet-js';

const checkPortal = async (portal, skylink) => {
  const client = new SkynetClient(portal);
  let portalUrl = undefined;

  try {
    // as long as no error, assume file is there;
    await client.getMetadata(skylink, { timeout: 4 });
    // const response = await client.getMetadata(skylink);

    portalUrl = client.getSkylinkUrl(skylink);
  } catch (error) {
    return portalUrl;
  }

  return portalUrl;
};

const useSkynetPortalCheck = (portals) => {
  const [status, setStatus] = useState('');
  const [portalAlternatives, setPortalAlternatives] = useState([]);

  const findPortals = async (skylink) => {
    try {
      let validPortals = [];
      setPortalAlternatives([]);
      setStatus('checking');

      Promise.all(
        portals.map(async (portal) => {
          console.log('checking: ', portal);
          let p = await checkPortal(portal, skylink).catch((e) => {});

          if (p !== undefined) {
            console.log('found: ', p);
            validPortals.push(p);
            setPortalAlternatives([...validPortals]);
          } else {
            console.log('failure');
          }

          return p;
        })
      ).then((e) => {
        setStatus('completed');
      });

      // console.log("after dec:", portalUrls);
    } catch (error) {
      setStatus('error');
    }
  };

  return [portalAlternatives, status, findPortals];
};

export default useSkynetPortalCheck;
