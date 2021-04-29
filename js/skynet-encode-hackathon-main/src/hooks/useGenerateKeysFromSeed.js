import { genKeyPairFromSeed } from 'skynet-js';
import { useState } from 'react';

const useGenerateKeysFromSeed = () => {
  const [generatedPublicKey, setPublicKey] = useState('');
  const [generatedPrivateKey, setPrivateKey] = useState('');

  const generateKeysFromSeed = (seed) => {
    const { publicKey, privateKey } = genKeyPairFromSeed(seed);

    setPublicKey(publicKey);
    setPrivateKey(privateKey);
  };

  return [generatedPublicKey, generatedPrivateKey, generateKeysFromSeed];
};

export default useGenerateKeysFromSeed;
