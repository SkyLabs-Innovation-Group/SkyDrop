import React, { useState, useEffect } from 'react';
import useDebounce from '../hooks/useDebounce';
import useResolveHNS from '../hooks/useResolveHNS';
import useBase32Subdomain from '../hooks/useBase32Subdomain';
import { Input, List, Typography, Divider } from 'antd';

// import "./HNSLookup.css";

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
  color: '#b!importantdbdbd',
  outline: 'none',
  transition: 'border .24s ease-in-out',
  // minWidth: "80vw"
};

const HNSLookup = (props) => {
  const [hnsTerm, setHnsTerm] = useState('');
  const hnsUrl = useDebounce(hnsTerm, 500);
  const [
    resolvedHNSUrl,
    registryEntryUrl,
    status,
    resolveHns,
  ] = useResolveHNS();
  const [base32SubdomainUrl, base32SubdomainLookup] = useBase32Subdomain();
  const [links, setLinks] = useState([]);

  // when input is changed (after debounce) resolve HNS
  useEffect(() => {
    resolveHns(hnsUrl);
  }, [hnsUrl]);

  // when resolved HNS changes, get base32 subdomain
  useEffect(() => {
    base32SubdomainLookup(resolvedHNSUrl);
  }, [resolvedHNSUrl]);

  // when base32 subdomain changes, make a list of links to display
  useEffect(() => {
    setLinks([]);
    const makeLinkList = () => {
      let linkList = [];

      if (registryEntryUrl)
        linkList.push({ name: 'Registry Entry', href: registryEntryUrl });
      if (resolvedHNSUrl)
        linkList.push({ name: resolvedHNSUrl, href: resolvedHNSUrl });
      if (base32SubdomainUrl)
        linkList.push({ name: base32SubdomainUrl, href: base32SubdomainUrl });

      setLinks(linkList);
    };
    makeLinkList();
  }, [base32SubdomainUrl]);

  return (
    <div className="PortalAlternatives-container">
      <div className="baseStyle" style={baseStyle}>
        <Title level={5} type="secondary">
          Type Your HNS Name Below to Resolve
        </Title>
        <Input
          placeholder="encode-skynet/"
          value={hnsTerm}
          onChange={(e) => setHnsTerm(e.target.value)}
          disabled={status === 'checking'}
        />

        <div className="PortalAlternatives-links">
          <Divider />

          {status === 'error' && !links.length && (
            <Title level={5} type="secondary">
              None Found.
            </Title>
          )}

          <List
            loading={status === 'checking'}
            size="small"
            // bordered
            dataSource={links.length ? links : [{ href: '#', name: '' }]}
            renderItem={(item) => (
              <List.Item>
                <Link href={item.href} target="_blank">
                  {item.name}
                </Link>
              </List.Item>
            )}
          />
        </div>
      </div>
    </div>
  );
};

export default HNSLookup;
