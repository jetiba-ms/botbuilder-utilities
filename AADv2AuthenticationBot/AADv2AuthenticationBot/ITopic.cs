﻿using Microsoft.Bot.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace aadv2
{
    public interface ITopic
    {
        /// <summary>
        /// Name of the topic
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Called when topic starts
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> StartTopic(IBotContext context);

        /// <summary>
        /// called while topic active
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> ContinueTopic(IBotContext context);

        /// <summary>
        ///  Called when a topic is resumed
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task<bool> ResumeTopic(IBotContext context);
    }
}
