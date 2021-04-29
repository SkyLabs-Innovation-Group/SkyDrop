import SyntaxHighlighter from 'react-syntax-highlighter';
import { Container, Grid, Header, Segment } from 'semantic-ui-react';

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

const Part0 = ({ codeColor }) => {
  return (
    <>
      <Header
        as="h2"
        content="Prerequisites &amp; Setup"
        style={style.h2}
        textAlign="center"
      />

      <Container>
        <Grid verticalAlign="middle" relaxed columns={2} divided>
          <Grid.Row>
            <Grid.Column>
              <Header
                as="h3"
                content="Code"
                style={style.h3}
                textAlign="center"
              />
              <Segment>
                <Header as="h4">
                  <a target="_blank" href="https://nodejs.org/en/download/">
                    NodeJS Download
                  </a>
                </Header>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Header
                as="h3"
                content="Notes"
                style={style.h3}
                textAlign="center"
              />
              <Segment>
                Installation varies by platform and may take a bit.
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="console" style={codeColor}>
                  {'npm install -g yarn'}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>Installs a popular NodeJS package manager.</Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="console" style={codeColor}>
                  {
                    'git clone "https://github.com/NebulousLabs/skynet-workshop.git" \ncd skynet-workshop'
                  }
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                Copies files needed to do workshop and change to directory.
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="console" style={codeColor}>
                  {'yarn install'}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                Installs the packages necessary for our project based on{' '}
                <code>create-react-app</code>. This may take a while.
              </Segment>
            </Grid.Column>
          </Grid.Row>
          <Grid.Row>
            <Grid.Column>
              <Segment>
                <SyntaxHighlighter language="console" style={codeColor}>
                  {'yarn start'}
                </SyntaxHighlighter>
              </Segment>
            </Grid.Column>
            <Grid.Column>
              <Segment>
                Builds a developer build of our webapp and deploys it to{' '}
                <code>localhost:3000</code>.<br />
                <em>
                  (Use <code>Ctrl+C</code> in your terminal to stop your app.)
                </em>
              </Segment>
            </Grid.Column>
          </Grid.Row>
        </Grid>
      </Container>
    </>
  );
};

export default Part0;
