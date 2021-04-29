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

const step4_1 = `"homepage": ".",`;
const step4_2 = `// Initiate the SkynetClient
const client = new SkynetClient();`;
const step4_3 = `yarn build`;

const Part4 = ({ codeColor }) => {
  return (
    <>
      <Header
        as="h2"
        content="Part 4: Deploy the Web App on Skynet"
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
            <Divider vertical>4.1</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="json" style={codeColor}>
                  {step4_1}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                This tells React where your root directory is and helps with
                relative linking of content in the application.
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>4.2</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="javascript" style={codeColor}>
                  {step4_2}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                By not setting a default portal, SkynetClient will interact with
                whatever portal the deployed application is being served from.
                <br />
                <br />
                <em>
                  We can't leave this blank on localhost, because our
                  development environment isn't a Skynet Web Portal.
                </em>
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Divider vertical>4.3</Divider>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="console" style={codeColor}>
                  {step4_3}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                This builds the app into a bundle in the <code>build</code>{' '}
                folder.
                <br />
                <br />
                To deploy, upload the folder using the uploader on{' '}
                <a href="https://siasky.net" target="_blank">
                  https://siasky.net/
                </a>{' '}
                and visit the link.
                <br />
                <br />
                <em>
                  As you continue to work with Skynet, you'll discover tools
                  that allow more automated deployment techniques.
                </em>
              </Segment>
            </Grid.Column>
          </Grid.Row>
        </Grid>
      </Container>
    </>
  );
};

export default Part4;
