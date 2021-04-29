import React, { useState, useEffect } from 'react';
import useSkynetPortalCheck from '../hooks/useSkynetPortalCheck';
import useDebounce from '../hooks/useDebounce';
import { Input, List, Typography, Divider } from 'antd';

import './PortalAlternatives.css';
import { parseSkylink } from 'skynet-js';

const { Title, Link } = Typography;

const baseStyle = {
  flex: 1,
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  padding: '20px',
  borderWidth: 2,
  borderRadius: 2,
  borderColor: '#eeeeee',
  borderStyle: 'dashed',
  backgroundColor: '#fafafa',
  color: '#bdbdbd',
  outline: 'none',
  transition: 'border .24s ease-in-out',
  // minWidth: "80vw"
};

const portalList = [
  'https://siasky.net',
  'https://www.siacdn.com',
  'https://skynethub.io',
  'https://sialoop.net',
  'https://skydrain.net',
  'https://skynet.tutemwesi.com',
  'https://skynet.luxor.tech',
  'https://vault.lightspeedhosting.com',
  'https://skynet.utxo.no',
  'https://skyportal.xyz',
  'https://skynet.developmomentum.com',
];

const PortalAlternatives = (props) => {
  const [skyfileUrlTerm, setSkyfileUrlTerm] = useState('');
  const [portals, setPortals] = useState(portalList);
  const [portalAlternatives, status, findPortals] = useSkynetPortalCheck(
    portals
  );
  const skyfileUrl = useDebounce(skyfileUrlTerm, 500);

  useEffect(() => {
    // check siastats for portal list
    // https://siastats.info/dbs/skynet_current.json
    // if successful, parse and setPortals, else just drop it.
  }, []);

  useEffect(() => {
    if (parseSkylink(skyfileUrl)) {
      findPortals(parseSkylink(skyfileUrl));
    }
  }, [skyfileUrl]);

  return (
    <div className="PortalAlternatives-container">
      <div className="baseStyle" style={baseStyle}>
        <Title level={5} type="secondary">
          Paste your Skyfile URL Below for Portal Alternatives
        </Title>
        <Input
          placeholder="Skyfile URL"
          value={skyfileUrlTerm}
          onChange={(e) => setSkyfileUrlTerm(e.target.value)}
          disabled={status === 'checking'}
        />

        <div className="PortalAlternatives-links">
          <Divider />

          <List
            loading={status === 'checking'}
            size="small"
            // bordered
            dataSource={portalAlternatives.length ? portalAlternatives : ['']}
            renderItem={(item) => (
              <List.Item>
                <Link href={item} target="_blank">
                  {item}
                </Link>
              </List.Item>
            )}
          />
        </div>
      </div>
    </div>
  );
};

export default PortalAlternatives;
