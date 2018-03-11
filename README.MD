# MSIL Weaving

## Définition

Vous pouvez consulter la page [MSIL Injection](https://www.postsharp.net/aop.net/msil-injection) de PostSharp pour avoir une définition claire du concept.

## Outillage

Il existe plusieurs outils et frameworks permettant de transformer le code IL ou d'avoir une approche [AOP](https://en.wikipedia.org/wiki/Aspect-oriented_programming). Certains outils agissent post-compile, d'autres au runtime.

Dans ce papier, nous allons voir de petits exemples autour de [Mono.Cecil](www.mono-project.com/Cecil/) et [Fody](https://github.com/Fody/Fody), pour transformer et injecter du code IL.

Mono.Cecil est **LA** référence en la matière ! La plupart des outils et frameworks se basent sur ce dernier. \
[PostShap](https://www.postsharp.net/) étant l'outil incontournable dans ce domaine.

## Workshops

- [Mono.Cecil / Exemple simple](Workshop-Mono.Cecil-Simple/)
- [Mono.Cecil / Exemple avancé](Workshop-Mono.Cecil-Advanced/)
- [Fody](Workshop-Fody/)