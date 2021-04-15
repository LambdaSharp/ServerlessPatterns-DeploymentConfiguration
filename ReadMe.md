# Deployment Configuration

This repository shows how modules are configured for deployment.

## Basic Parameter Type

The basic parameter types for CloudFormation templates are `String`, `Number`, `CommaDelimitedList`, and `List<Number>`. In addition, CloudFormation has a dozen [AWS-specific parameter types](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/parameters-section-structure.html#aws-specific-parameter-types). For example, the type `AWS::EC2::VPC::Id` must correspond to an existing VPC.

Finally, there a few [SSM parameter types](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/parameters-section-structure.html#aws-ssm-parameter-types) that interact with the _Parameter Store_. However, these parameter types are not recommended. Instead, either use a parameter file, which can resolve values from the _Parameter Store_ and other locations, or use [dynamic references](https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/dynamic-references.html) (e.g. `{{resolve:ssm:S3AccessControl:2}}`) instead as the parameter value.

The `Parameter` declaration is used to define module parameters.

```yaml
- Parameter: TopicDisplayName
  Description: Display Name
  Section: Sample Parameters Settings
  Label: Display name of SNS topic
  Type: String
  Scope: stack
```

The `Section` property is used to group parameters together into section of the same name. The `Label` property is an additional hint that is shown for the expected value. The `Description` property is shown when the value is scoped to either `public` or `stack`.

```
cd BasicParameterType
lash deploy
```

## Resource-as-Parameter

The _Resource-as-Parameter_ declaration makes it possible to either pass in an existing resource via ARN, or, when no value is provided, the resource is instantiated instead.

_Resource-as-Parameter_ types requires an AWS type name as `Type` value and a `Properties` section. If no properties are required for the AWS resource, use `Properties: { }` to have an empty properties specification.

```yaml
- Parameter: MyTopic
  Type: AWS::SNS::Topic
  Properties:
    DisplayName: MyTopicDisplayName
```

**NOTES:**
* The LambdaSharp compiler automatically converts `!Ref MyTopic` to a conditional expression that either selects the input parameter value or the ARN (when available) of the AWS resource using `!GetAtt`. If the AWS resources does not have an ARN attribute, the default return value of the resource is used instead.
* It is not possible to use a _Resource-as-Parameter_ in the `!GetAtt` function.

## Secret Parameter

Sensitive information should be passed in using the `Secret` parameter type. These parameter values automatically have the `NoEcho` property enabled to not show their value in the UI, CLI, or API. Secret parameter values must be encoded using a KMS key. Furthermore, the CloudFormation stack must be granted `kms:Decrypt` permission to the KMS key. This is achieved by specifying the KMS key in the module using the `Secrets` declaration or by passing the KMS key in the `Secrets` parameter. The latter is the preferred method.

Lambda functions automatically decrypt the secret values on startup once and then cache the result in memory. This ensure that the decrypted secret value is not visible in the Lambda configuration in the AWS Console.

For resources that need a password or access token, append the `::Plaintext` suffix to the parameter name to dynamically decrypt the secret value during the CloudFormation stack execution.

```yaml
- Parameter: MySecretParameter
  Type: Secret

- Variable:
  Value: !Ref MySecretParameter::Plaintext
```

## Parameter File

Parameter value can be specified in a YAML parameter file. Values can either be specified explicitly or they can be looked up from other sources like a JSON configuration file, the _Parameter Store_, or environment variables.

```yaml
# set SNS topic name
TopicDisplayName: !GetConfig
  - !Sub [ "env-${Target}.json", { Target: !GetEnv "TARGET_ENV" } ]
  - TopicSettings.DisplayName

# grant access to required KMS keys
Secrets:
  - alias/MySecretKey
```

### Parameter Functions

The following parameter functions are available in the YAML file.

* `!GetConfig [ json-file-path, json-path-expression ]`
  * Opens the JSON file at `json-file-path` and read the value a the `json-path-expression`.
* `!GetEnv environment-variable`
  * Read the value of an environment variable.
* `!GetParam parameter-store-path` -OR- `!GetParam [ parameter-store-path ]`
  * Read a value from the parameter store. If the value is a `SecureString`, it will be decrypted.
* `!GetParam [ parameter-store-path, encryption-key-id ]`
  * Read a value from the parameter store. If the value is a `SecureString`, it will be decrypted. Re-encrypt the value using the KMS key identified by `encryption-key-id`.
* `!Sub format-string` -OR- `!Sub [ format-string, arguments ]`
  * Build a new parameter value from other values.
* `!Ref` can be used to resolve the following builtin-variable
    * `Deployment::BucketName`
    * `Deployment::Tier`
    * `Deployment::TierLowercase`
    * `Deployment::TierPrefix`
    * `Deployment::TierPrefixLowercase`

