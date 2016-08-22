﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Alm.Cli.Test
{
    [TestClass]
    public class OperationArgumentsTests
    {
        [TestMethod]
        public void Typical()
        {
            const string input = "protocol=https\n"
                               + "host=example.visualstudio.com\n"
                               + "path=path\n"
                               + "username=userName\n"
                               + "password=incorrect\n";

            OperationArguments cut;
            using (var memory = new MemoryStream())
            using (var writer = new StreamWriter(memory))
            {
                writer.Write(input);
                writer.Flush();

                memory.Seek(0, SeekOrigin.Begin);

                cut = new OperationArguments(memory);
            }

            Assert.AreEqual("https", cut.QueryProtocol);
            Assert.AreEqual("example.visualstudio.com", cut.QueryHost);
            Assert.AreEqual("https://example.visualstudio.com/", cut.TargetUri.ToString());
            Assert.AreEqual("path", cut.QueryPath);
            Assert.AreEqual("userName", cut.CredUsername);
            Assert.AreEqual("incorrect", cut.CredPassword);

            var expected = ReadLines(input);
            var actual = ReadLines(cut.ToString());
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SpecialCharacters()
        {
            const string input = "protocol=https\n"
                               + "host=example.visualstudio.com\n"
                               + "path=path\n"
                               + "username=userNamể\n"
                               + "password=ḭncorrect\n";

            OperationArguments cut;
            using (var memory = new MemoryStream())
            using (var writer = new StreamWriter(memory))
            {
                writer.Write(input);
                writer.Flush();

                memory.Seek(0, SeekOrigin.Begin);

                cut = new OperationArguments(memory);
            }

            Assert.AreEqual("https", cut.QueryProtocol);
            Assert.AreEqual("example.visualstudio.com", cut.QueryHost);
            Assert.AreEqual("https://example.visualstudio.com/", cut.TargetUri.ToString());
            Assert.AreEqual("path", cut.QueryPath);
            Assert.AreEqual("userNamể", cut.CredUsername);
            Assert.AreEqual("ḭncorrect", cut.CredPassword);

            var expected = ReadLines(input);
            var actual = ReadLines(cut.ToString());
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void CreateTargetUri_GithubSimple()
        {
            var input = new InputArg()
            {
                Protocol = "https",
                Host = "github.com",
            };

            CreateTargetUriTest(input);
        }

        [TestMethod]
        public void CreateTargetUri_VstsSimple()
        {
            var input = new InputArg()
            {
                Protocol = "https",
                Host = "team.visualstudio.com",
            };

            CreateTargetUriTest(input);
        }

        [TestMethod]
        public void CreateTargetUri_GithubComplex()
        {
            var input = new InputArg()
            {
                Protocol = "https",
                Host = "github.com",
                Path = "Microsoft/Git-Credential-Manager-for-Windows.git"
            };

            CreateTargetUriTest(input);
        }

        [TestMethod]
        public void CreateTargetUri_WithPortNumber()
        {
            var input = new InputArg()
            {
                Protocol = "https",
                Host = "onpremis:8080",
            };

            CreateTargetUriTest(input);
        }

        [TestMethod]
        public void CreateTargetUri_ComplexAndMessy()
        {
            var input = new InputArg()
            {
                Protocol = "https",
                Host = "foo.bar.com:8181?git-credential=manager",
                Path = "this-is/a/path%20with%20spaces",
            };

            CreateTargetUriTest(input);
        }

        [TestMethod]
        public void CreateTargetUri_WithCredentials()
        {
            var input = new InputArg()
            {
                Protocol = "http",
                Host = "insecure.com",
                Username = "naive",
                Password = "password",
            };

            CreateTargetUriTest(input);
        }

        private void CreateTargetUriTest(InputArg input)
        {
            using (var memory = new MemoryStream())
            using (var writer = new StreamWriter(memory))
            {
                writer.Write(input.ToString());
                writer.Flush();

                memory.Seek(0, SeekOrigin.Begin);

                var oparg = new OperationArguments(memory);

                Assert.IsNotNull(oparg);
                Assert.AreEqual(input.Protocol, oparg.QueryProtocol);
                Assert.AreEqual(input.Host, oparg.QueryHost);
                Assert.AreEqual(input.Path, oparg.QueryPath);
                Assert.AreEqual(input.Username, oparg.CredUsername);
                Assert.AreEqual(input.Password, oparg.CredPassword);
            }
        }

        private static ICollection ReadLines(string input)
        {
            var result = new List<string>();
            using (var sr = new StringReader(input))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    result.Add(line);
                }
            }
            return result;
        }

        struct InputArg
        {
            public string Protocol;
            public string Host;
            public string Path;
            public string Username;
            public string Password;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();

                sb.Append("protocol=").Append(Protocol).Append("\n");
                sb.Append("host=").Append(Host).Append("\n");

                if (Path != null)
                {
                    sb.Append("path=").Append(Path).Append("\n");
                }
                if (Username != null)
                {
                    sb.Append("username=").Append(Username).Append("\n");
                }
                if (Password != null)
                {
                    sb.Append("password=").Append(Password).Append("\n");
                }

                sb.Append("\n");

                return sb.ToString();
            }
        }
    }
}
