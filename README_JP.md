# FSharp.LintKit

**AI駆動ルール実装**を可能にする強力なF#カスタムリントフレームワークです。付属のAI指示テンプレートにルールを指定することで、AIエージェントが包括的なテスト付きのアナライザー実装を生成できます。

## 概要

FSharp.LintKitは、F#ソースコードにカスタムリントルールを適用するための基盤を提供します。従来のリンティングツールとは異なり、**チーム固有・プロジェクト固有のルール**にフォーカスし、AIエージェントによるルール開発を支援します。

### なぜFSharp.LintKit？
- **AI対応テンプレート**: AIエージェント向け包括的指示テンプレートで実装生成
- **ゼロコーディング**: ルール実装に手動F#コーディング不要
- **簡単配布**: 標準.NETツールチェーンとNuGetパッケージ
- **動的ローディング**: カスタムアナライザーDLLの実行時ロード
- **チーム特化**: プロジェクト規約とガイドライン強制に最適

## 主要機能

- **AI駆動ルール実装**: 仕様から動作アナライザーまでの自動化
- **動的アナライザーローディング**: 複数のカスタムルールDLLを実行時ロード
- **包括的テスト**: 自動生成テストスイート
- **複数出力フォーマット**: CI/CD統合用のプレーンテキストとSARIF
- **クロスプラットフォーム**: Windows、macOS、Linux対応
- **エンタープライズ対応**: 内部NuGetフィードサポート

## クイックスタート

### 1. ツールのインストール
```bash
# CLIツールのインストール
dotnet tool install -g FSharp.LintKit

# プロジェクトテンプレートのインストール
dotnet new install FSharp.LintKit.Templates
```

### 2. カスタムアナライザーの作成
```bash
# 新しいアナライザープロジェクトを作成
dotnet new fsharplintkit-analyzer -n MyProjectRules

# プロジェクトに移動
cd MyProjectRules
```

### 3. AIでルール指定
1. `AI_RULE_IMPLEMENTATION.md`を開く
2. ルール仕様を記入:
```markdown
### ルール1: Console.WriteLineの禁止
ルール名: NoConsoleWriteLine
カテゴリ: CodeQuality
重要度: Warning
説明: Console.WriteLineは本番コードで使用すべきではない
検出パターン: Console.WriteLine関数呼び出し
メッセージ: Console.WriteLineの代わりに適切なロギングを使用してください
```

3. ドキュメント全体をAIエージェントに送信:
> "上記のルール仕様に基づいてF#アナライザーを実装してください"

### 4. ビルドと使用
```bash
# 生成されたアナライザーをビルド
dotnet build

# リント解析を実行
dotnet fsharplintkit --analyzers ./bin/Debug/net8.0/MyProjectRules.dll --target ./src
```

## インストール

### 前提条件
- .NET 8.0以上
- AIエージェントへのアクセス（Claude、ChatGPT等）

### CLIツールのインストール
```bash
dotnet tool install -g FSharp.LintKit
```

### プロジェクトテンプレートのインストール
```bash
dotnet new install FSharp.LintKit.Templates
```

## 使用方法

### 基本的なコマンドライン使用法
```bash
# カスタムルールで解析
dotnet fsharplintkit --analyzers path/to/rules.dll --target ./src

# 複数アナライザー
dotnet fsharplintkit --analyzers rules1.dll --analyzers rules2.dll --target ./src

# CI用SARIF出力
dotnet fsharplintkit --analyzers rules.dll --target ./src --format sarif

# 詳細出力
dotnet fsharplintkit --analyzers rules.dll --target ./src --verbose
```

### サポートされるターゲット
- **ソリューションファイル**: `--target MyProject.sln`
- **プロジェクトファイル**: `--target MyProject.fsproj`
- **ディレクトリ**: `--target ./src` （再帰的.fsファイル検索）
- **単一ファイル**: `--target MyFile.fs`

### 出力フォーマット
- **text**: 人間が読みやすいコンソール出力（デフォルト）
- **sarif**: CI/CD統合用SARIFフォーマット

## AI駆動ルール実装

### 完全自動化ワークフロー

1. **人間の入力**: `AI_RULE_IMPLEMENTATION.md`テンプレートでルールを記述
2. **AIエージェント**: テンプレートをAIエージェントに送信して実装
3. **統合**: 生成されたアナライザーがLintKit.CLIで動作

### ルール仕様フォーマット
```markdown
ルール名: [わかりやすい名前]
カテゴリ: [CodeQuality/Security/Performance/Naming/Style]
重要度: [Error/Warning/Info/Hint]
説明: [何を検出するか、なぜ重要か]
検出パターン: [具体的なF#コードパターン]
除外条件: [無視するパターン]
メッセージ: [ユーザー向けメッセージ]
修正提案: [推奨される修正]
```

## 使用例

### コード品質ルール
- 過度に複雑な関数の検出
- 命名規約の強制
- 深いネストの防止
- 適切なエラーハンドリングの確保

### チーム規約ルール
- プロジェクト固有パターンの強制
- ドキュメント標準の確保
- アーキテクチャ決定の検証
- コード組織の確認

## アーキテクチャ

### コアコンポーネント
- **LintKit.CLI**: コマンドラインインターフェースとアナライザーエンジン
- **Templates**: 完全なASTパターンを含むカスタムアナライザー用NuGetプロジェクトテンプレート

### アナライザーフレームワーク
- **FSharp.Analyzers.SDK**をベースに構築
- **FSharp.Compiler.Syntax**によるF# AST解析
- 実行時の動的DLLローディング
- メッセージと診断レポート

### AI支援
- AIエージェント向け動作パターン参照
- 包括的指示ドキュメント
- テンプレートベースコード生成
- 自動テストスイート作成

## ドキュメント

### 人間向け
- **日本語**: `templates/RULE_IMPLEMENTATION_GUIDE_JA.md`
- **英語**: `templates/RULE_IMPLEMENTATION_GUIDE_EN.md`

### AIエージェント向け
- **実装指示**: `templates/MyCustomAnalyzer/AI_RULE_IMPLEMENTATION.md`
- **完全なASTパターン**: `templates/MyCustomAnalyzer/TemplateAnalyzer.fs`
- **テストパターン**: `templates/MyCustomAnalyzer/TemplateAnalyzerTests.fs`

### 技術リファレンス
- **アーキテクチャ詳細**: F# AST解析パターン
- **テストパターン**: FSharp.Analyzers.SDK.Testing使用法

## 開発

### ソースからのビルド
```bash
git clone https://github.com/yourorg/FSharp.LintKit.git
cd FSharp.LintKit
dotnet build
dotnet test
```

### プロジェクト構造
```
FSharp.LintKit/
├── src/
│   └── LintKit.CLI/              # コマンドラインインターフェース
├── templates/
│   └── MyCustomAnalyzer/         # 完全なASTパターンを含むNuGetプロジェクトテンプレート
├── tests/
│   └── LintKit.Tests/            # CLIコンポーネントのユニットテスト
└── README.md                     # このファイル
```

## ライセンス

このプロジェクトはMITライセンスの下でライセンスされています - 詳細は[LICENSE](LICENSE)ファイルを参照してください。
