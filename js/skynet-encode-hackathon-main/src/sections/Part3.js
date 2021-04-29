import SyntaxHighlighter from 'react-syntax-highlighter';
import { Container, Grid, Header, Segment, Divider } from 'semantic-ui-react';

const style = {
  h1: {
    marginTop: '3em',
  },
  h2: {
    margin: '4em 0em 2em',
  },
  h3: {
    // marginTop: '2em',
    // padding: '2em 0em',
  },
  last: {
    marginBottom: '300px',
  },
};

const step3_1 = `import { genKeyPairFromSeed } from 'skynet-js';`;
const step3_2 = `// Generate the user's private and public keys
const { privateKey, publicKey } = genKeyPairFromSeed(seed);

// Create an object to write to SkyDB
// Conversion to JSON happens automatically.
const jsonData = {
  name,
  skylinkUrl,
  dirSkylinkUrl,
  color: userColor,
};

// Use setJSON to save the user's information to SkyDB
try {
  await client.db.setJSON(privateKey, dataKey, jsonData);
} catch (error) {
  console.log(\`error with setJSON: \${error.message}\`);
}

// Let's get see info on our SkyDB entry
console.log('SkyDB Entry Written--');
console.log('Public Key: ', publicKey);
console.log('Data Key: ', dataKey);`;
const step3_3 = `console.log('Saving user data to SkyDB...');`;
const step3_4 = `const webPage = generateWebPage(name, skylinkUrl, seed, dataKey);`;
const step3_5 = `// Generate the user's public key again from the seed.
const { publicKey } = genKeyPairFromSeed(seed);

// Use getJSON to load the user's information from SkyDB
const { data } = await client.db.getJSON(publicKey, dataKey);

// To use this elsewhere in our React app, save the data to the state.
if (data) {
  setName(data.name);
  setFileSkylink(data.skylinkUrl);
  setWebPageSkylink(data.dirSkylinkUrl);
  setUserColor(data.color);
  console.log('User data loaded from SkyDB!');
} else {
  console.error('There was a problem with getJSON');
}`;

const Part3 = ({ codeColor }) => {
  return (
    <>
      <Header
        as="h2"
        content="Part 3: Make it Dynamic with SkyDB"
        style={style.h2}
        textAlign="center"
      />

      <Container>
        <Grid verticalAlign="middle" relaxed columns={2}>
          <Grid.Row>
            <Grid.Column>
              <Header
                as="h3"
                content="Code"
                style={style.h3}
                textAlign="center"
              />
            </Grid.Column>
            <Grid.Column>
              <Header
                as="h3"
                content="Notes"
                style={style.h3}
                textAlign="center"
              />
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>3.1</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="javascript" style={codeColor}>
                  {step3_1}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                This import will let us take a "seed phrase" and create a public
                and private key pair.
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>3.2</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="javascript" style={codeColor}>
                  {step3_2}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                Here we generate a pair of keys for use in uploading our JSON
                data. Later, anyone with the publicKey and dataKey can view this
                content, but only someone with the privateKey can modify it.
                <br />
                <br />
                Our certificate will have our publicKey and dataKey hard-coded
                so it can read any changes to our chosen color, even after it's
                published.
                <br />
                <br />
                <em>
                  Color is a trivial example, but this feature is very powerful
                  when used for user profiles, blogposts and whatever
                  application data your app may need.
                </em>
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>3.3</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="javascript" style={codeColor}>
                  {step3_3}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                We'll uncomment this line so we can keep track of what's
                happening in our code.
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>3.4</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="javascript" style={codeColor}>
                  {step3_4}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                We need to pass the seed and dataKey to our page generator so it
                knows where to look for our SkyDB entry.
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>3.5</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="javascript" style={codeColor}>
                  {step3_5}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                This code is called in the <code>loadData</code> function when
                the "Load Data" button is pressed.
                <br />
                <br />
                Using a publicKey (derived from the provided seed) and a
                dataKey, it looks up any JSON data previously saved to SkyDB and
                sets our application state accordingly.
              </Segment>
            </Grid.Column>
          </Grid.Row>
        </Grid>
      </Container>
    </>
  );
};

export default Part3;
