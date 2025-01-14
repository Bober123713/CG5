#version 330 core

out vec4 FragColor;

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

uniform vec3 objectColor;
uniform vec3 lightColor;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform float ambientStr;
uniform float specularStr;
uniform int specularCoeff;

uniform sampler2D texture1;

void main() {
    /* diffuse */
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(lightPos - FragPos);

    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;

    /* specular */
    vec3 viewDir = normalize(viewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), specularCoeff);
    vec3 specular = specularStr * spec * lightColor;

    /* ambient */
    vec3 ambient = ambientStr * lightColor;

    vec3 color;
    
    vec3 result = (ambient + diffuse + specular) * texture(texture1, TexCoord).xyz;

    FragColor = vec4(result, 1.0f);
}