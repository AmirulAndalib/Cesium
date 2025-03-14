// SPDX-FileCopyrightText: 2025 Cesium contributors <https://github.com/ForNeVeR/Cesium>
//
// SPDX-License-Identifier: MIT

namespace Cesium.Preprocessor;

public interface IIncludeContext
{
    bool ShouldIncludeFile(string filePath);
    void RegisterGuardedFileInclude(string filePath);
    string LookUpAngleBracedIncludeFile(string filePath);
    string LookUpQuotedIncludeFile(string filePath);
    /// <returns><c>null</c> if the target file doesn't exist.</returns>
    TextReader? OpenFileStream(string filePath);
}
