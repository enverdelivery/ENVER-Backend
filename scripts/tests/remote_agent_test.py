"""Pure/local validation of the fixed SSM agent. No AWS/network/service/container calls."""
import importlib.util
from pathlib import Path
import tempfile
import unittest
from unittest.mock import mock_open, patch


SOURCE = Path(__file__).resolve().parents[1] / "deploy" / "remote_agent.py"
spec = importlib.util.spec_from_file_location("enver_remote_agent", SOURCE)
agent = importlib.util.module_from_spec(spec)
spec.loader.exec_module(agent)


class RemoteAgentTests(unittest.TestCase):
    def test_invalid_operation_stops_before_metadata_or_commands(self):
        with patch.dict(agent.os.environ, {"SSM_Operation": "arbitrary-shell"}), patch.object(agent, "metadata") as metadata, patch.object(agent.os, "geteuid", return_value=0):
            with self.assertRaises(RuntimeError):
                agent.main()
            metadata.assert_not_called()

    def test_root_is_required_before_any_operation(self):
        with patch.object(agent.os, "geteuid", return_value=1654), patch.object(agent, "metadata") as metadata:
            with self.assertRaises(RuntimeError):
                agent.main()
            metadata.assert_not_called()

    def test_mutable_tags_and_shell_injection_are_rejected(self):
        for value in ("latest", "sha256:bad", "sha256:" + "a" * 64 + "; echo forbidden"):
            with patch.dict(agent.os.environ, {"SSM_Operation": "deploy", "SSM_DesiredDigest": value, "SSM_PreviousDigest": "sha256:" + "b" * 64, "SSM_DeploymentId": "123-1"}), patch.object(agent, "metadata") as metadata, patch.object(agent.os, "geteuid", return_value=0):
                with self.assertRaises(RuntimeError):
                    agent.main()
                metadata.assert_not_called()

    def test_reference_accepts_only_exact_expected_registry_digest(self):
        with tempfile.TemporaryDirectory() as directory:
            file = Path(directory) / "image.env"
            valid = "ENVER_API_IMAGE=" + agent.REPOSITORY + "@sha256:" + "a" * 64 + "\n"
            with patch.object(agent, "REFERENCE", str(file)), patch.object(agent, "protected"):
                file.write_text(valid)
                self.assertEqual(agent.reference(), "sha256:" + "a" * 64)
                for invalid in (valid + "EXTRA=1\n", valid.replace(agent.REGISTRY, "untrusted.invalid"), "ENVER_API_IMAGE=latest\n"):
                    file.write_text(invalid)
                    with self.assertRaises(RuntimeError):
                        agent.reference()

    def test_health_destination_is_fixed_before_network(self):
        with patch.object(agent.urllib.request, "urlopen") as network:
            with self.assertRaises(RuntimeError):
                agent.health("https://d3htt8nzgp6ksn.cloudfront.net/health")
            network.assert_not_called()

    def test_canonical_hash_input_is_stable(self):
        self.assertEqual(agent.canonical({"b": 2, "a": 1}), agent.canonical({"a": 1, "b": 2}))

    def test_failed_forward_before_journal_does_not_restart_previous_image(self):
        expected = "sha256:" + "a" * 64
        env = {"SSM_Operation": "restore", "SSM_DesiredDigest": expected, "SSM_PreviousDigest": expected, "SSM_DeploymentId": "123-1"}
        with patch.dict(agent.os.environ, env), patch.object(agent.os, "geteuid", return_value=0), patch.object(agent, "metadata", return_value=(next(iter(agent.HOSTS)), None)), patch.object(agent, "reference", return_value=expected), patch.object(agent.os.path, "exists", return_value=False), patch("builtins.open", mock_open()), patch.object(agent.fcntl, "flock"), patch.object(agent, "wait_ready") as ready, patch.object(agent, "pull") as pull, patch.object(agent, "atomic") as write:
            result, _, operation = agent.main()
            self.assertEqual(result["digest"], expected)
            self.assertEqual(operation, "restore")
            ready.assert_called_once()
            pull.assert_not_called()
            write.assert_not_called()


if __name__ == "__main__":
    unittest.main()
