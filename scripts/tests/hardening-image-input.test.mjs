import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import {command, sha256} from '../ci/lib.mjs';
import {inspectOCI} from '../ci/pipeline.mjs';
import {trivyImageArguments} from '../ci/hardening-diagnostics.mjs';

function fixture(body) {
  const dir = fs.mkdtempSync(path.join(os.tmpdir(), 'enver-oci-input-'));
  try {
    const source = path.join(dir, 'source');
    fs.mkdirSync(path.join(source, 'blobs', 'sha256'), {recursive: true});
    const rootfs = path.join(dir, 'rootfs');
    fs.mkdirSync(path.join(rootfs, 'etc'), {recursive: true});
    fs.writeFileSync(path.join(rootfs, 'etc', 'os-release'), 'ID=alpine\nVERSION_ID=3.21.0\n');
    const layerTar = path.join(dir, 'layer.tar');
    command('tar', ['-cf', './layer.tar', '-C', rootfs, 'etc'], {cwd: dir});
    function blob(bytes) {
      const hash = sha256(bytes);
      fs.writeFileSync(path.join(source, 'blobs', 'sha256', hash), bytes);
      return {digest: 'sha256:' + hash, size: bytes.length};
    }
    const layer = blob(fs.readFileSync(layerTar));
    const config = blob(Buffer.from(JSON.stringify({os: 'linux', architecture: 'amd64',
      config: {User: '1654:1654'}, rootfs: {type: 'layers', diff_ids: [layer.digest]}})));
    const manifest = blob(Buffer.from(JSON.stringify({schemaVersion: 2,
      mediaType: 'application/vnd.oci.image.manifest.v1+json',
      config: {...config, mediaType: 'application/vnd.oci.image.config.v1+json'},
      layers: [{...layer, mediaType: 'application/vnd.oci.image.layer.v1.tar'}]})));
    fs.writeFileSync(path.join(source, 'index.json'), JSON.stringify({schemaVersion: 2,
      manifests: [{...manifest, mediaType: 'application/vnd.oci.image.manifest.v1+json'}]}));
    fs.writeFileSync(path.join(source, 'oci-layout'), '{"imageLayoutVersion":"1.0.0"}');
    const archive = path.join(dir, 'image.tar');
    command('tar', ['-cf', './image.tar', '-C', source, 'index.json', 'oci-layout', 'blobs'], {cwd: dir});
    body({dir, source, archive, manifest});
  } finally { fs.rmSync(dir, {recursive: true, force: true}); }
}

test('OCI projection preserves canonical archive hash, exact manifest digest and all referenced bytes', () => fixture(({dir, source, archive, manifest}) => {
  const before = inspectOCI(archive);
  const layout = path.join(dir, 'trivy-image');
  assert.deepEqual(inspectOCI(archive, layout), before);
  assert.equal(before.manifestDigest, manifest.digest);
  assert.equal(sha256(fs.readFileSync(archive)), before.artifactHash);
  for (const file of ['index.json', 'oci-layout', ...fs.readdirSync(path.join(source, 'blobs', 'sha256')).map(name => 'blobs/sha256/' + name)]) {
    assert.deepEqual(fs.readFileSync(path.join(layout, file)), fs.readFileSync(path.join(source, file)));
    assert.equal(fs.lstatSync(path.join(layout, file)).isFile(), true);
    assert.equal(fs.lstatSync(path.join(layout, file)).isSymbolicLink(), false);
  }
  assert.ok(command('tar', ['-tf', './image.tar'], {cwd: dir}).stdout.includes('index.json'));
  assert.equal(trivyImageArguments[trivyImageArguments.indexOf('--input') + 1], '/private/trivy-image');
  assert.ok(trivyImageArguments.includes('vuln'));
}));
test('OCI projection rejects unexpected archive paths before writing any layout', () => fixture(({dir, source, archive}) => {
  fs.writeFileSync(path.join(source, 'unexpected.txt'), 'not an OCI entry');
  command('tar', ['-rf', './image.tar', '-C', source, 'unexpected.txt'], {cwd: dir});
  const layout = path.join(dir, 'rejected');
  assert.throws(() => inspectOCI(archive, layout), {code: 'OCI_PATHS'});
  assert.equal(fs.existsSync(layout), false);
}));
test('OCI projection never accepts a manifest whose bytes differ from its digest', () => fixture(({dir, source, archive, manifest}) => {
  fs.appendFileSync(path.join(source, 'blobs', 'sha256', manifest.digest.slice(7)), ' ');
  command('tar', ['-cf', './image.tar', '-C', source, 'index.json', 'oci-layout', 'blobs'], {cwd: dir});
  assert.throws(() => inspectOCI(archive, path.join(dir, 'rejected')), {code: 'OCI_MANIFEST_HASH'});
}));
