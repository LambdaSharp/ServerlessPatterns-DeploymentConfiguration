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

> TODO

* Encrypted values
    * Using `Secrets`
    * Using `lash encrypt`
    * Parameter type `Secret`
```yaml
- Parameter: MySecretParameter
  Type: Secret

- Variable:
  Value: !Ref MySecretParameter::Plaintext
```

## Parameter File

> TODO

* Parameter file
    * Types of parameters
        * key = value
        * key = list of values -> comma separated values
    * Lookup Functions
        * !GetConfig [ json-file-path, json-path-expression ]
        * !GetEnv environment-variable
        * !GetParam parameter-store-path -OR- !GetParam [ parameter-store-path ]
        * !GetParam [ parameter-store-path, encryption-key-id ]
        * !Ref builtin-variable
            * Deployment::BucketName
            * Deployment::Tier
            * Deployment::TierLowercase
            * Deployment::TierPrefix
            * Deployment::TierPrefixLowercase
        * !Sub format-string
        * !Sub [ format-string, arguments ]

* Configuration sources
    * Literal values
    * Parameter store
        * Re-encrypting secret values
    * Environment variable
    * JSON configuration file


















## BitcoinTopic

The _BitcoinTopic_ module creates a Lambda function that publishes the most recent bitcoin price on an SNS topic. The SNS topic is exported from the CloudFormation stack so that other stacks can subscribe to it.

## BitcoinTable

The _BitcoinTable_ module creates a Lambda function that subscribes to the SNS topic from the _BitcoinTopic_ stack and stores the received value in a DynamoDB table. Stored values are automatically forgotten after 15 minutes to minimize the number of stored rows. The DynamoDB table is exported for other stacks to query against.

## BitcoinActivity

The _BitcoinActivity_ module creates a REST API for recording buy/sell activity. It imports the table from the _BitcoinTable_ stack to fetch the most recently recorded price.

A Postman collection is provided to easily interact with the REST API.
